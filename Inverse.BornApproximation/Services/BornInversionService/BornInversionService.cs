using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Extensions;
using Electromagnetic.Common.Models;
using Inverse.BornApproximation.Services.JacobianService;
using Inverse.SharedCore.DirectTaskService;
using MathNet.Numerics.LinearAlgebra;

namespace Inverse.BornApproximation.Services.BornInversionService;

/// <summary>
/// Реализация метода Борновских приближений для решения обратной задачи.
/// </summary>
public class BornInversionService(
    IDirectTaskService directTaskService,
    IBornJacobianCacheService jacobianCacheService
) : IBornInversionService
{
    public async Task<double[]> InvertIterativelyAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        double[] observedValues,
        double baseMu,
        InverseOptions options,
        int maxIterations = 20,
        double functionalThreshold = 1e-8
    )
    {
        int n = mesh.Elements.Count;
        int m = sensors.Count;

        var mu = Enumerable.Repeat(baseMu, n).ToArray();
        double initialFunctional = double.MaxValue;
        double previousFunctional = double.MaxValue;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            Console.WriteLine($"\n[Born Iteration {iteration + 1}]");

            // Построение фоновой модели и базового отклика
            var mesh0 = mesh.CloneWithUniformMu(baseMu);
            var baseField = await directTaskService.CalculateDirectTaskAsync(mesh0, sensors, sources);
            var baseValues = baseField.Select(s => s.Magnitude).ToArray();
            
            // Построение якобиана на фоне (однократно)
            var J = await jacobianCacheService.BuildOnceAsync(mesh0, sensors, sources, baseField);
            var matJ = Matrix<double>.Build.DenseOfArray(J);
            var JT = matJ.Transpose();
            var JTJ = JT * matJ;
            var residualVectorTemplate = Vector<double>.Build.DenseOfEnumerable(
                observedValues.Zip(baseValues, (obs, model) => obs - model)
            );
            
            // Регуляризация (Тихонов 1)
            for (int i = 0; i < n; i++)
                JTJ[i, i] += options.Lambda;

            // Построение текущей модели
            for (int i = 0; i < n; i++)
                mesh.Elements[i].Mu = mu[i];

            // Расчёт отклика модели
            var currentField = await directTaskService.CalculateDirectTaskAsync(mesh, sensors, sources);
            var currentValues = currentField.Select(s => s.Magnitude).ToArray();

            // Расчёт невязки и функционала
            double functional = 0.0;
            for (int i = 0; i < m; i++)
            {
                var residual = observedValues[i] - currentValues[i];
                functional += residual * residual;
            }


            Console.WriteLine($"Functional: {functional:E8}");

            if (iteration == 0)
                initialFunctional = functional;
            else
            {
                var deltaF = previousFunctional - functional;

                Console.WriteLine($"Functional Δ: {deltaF:E8}");

                if (Math.Abs(deltaF) < functionalThreshold)
                {
                    Console.WriteLine("Stopping: functional change below threshold.");
                    break;
                }
            }

            previousFunctional = functional;

            // Используем фиксированный residual = observed - base
            var JTr = JT * residualVectorTemplate;

            // Решение СЛАУ
            var deltaMu = JTJ.Solve(JTr);

            // Обновление модели
            for (int i = 0; i < n; i++)
                mu[i] += deltaMu[i];
        }

        return mu;
    }
}