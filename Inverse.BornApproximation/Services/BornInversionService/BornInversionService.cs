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
/// <summary>
/// Итерационный метод Борна с фиксированным rot(A0) и обновлением правой части.
/// </summary>
public class BornInversionService(
    IDirectTaskService directTaskService,
    IBornJacobianCacheService jacobianCacheService
) : IBornInversionService
{
    public async Task<double[]> AdaptiveInvertAsync(
        IReadOnlyList<FieldSample> trueModelValues,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<FieldSample> primaryField,
        double baseMu,
        Mesh initialMesh,
        InverseOptions inversionOptions,
        MeshRefinementOptions refinementOptions
    )
    {
        int n = initialMesh.Elements.Count;
        int m = sensors.Count;
        double[] mu = Enumerable.Repeat(baseMu, n).ToArray();

        var observedValues = trueModelValues.Select(s => s.Magnitude).ToArray();

        // 1. Строим фоновую модель и решаем A0 (один раз)
        var mesh0 = initialMesh.CloneWithUniformMu(baseMu);
        var baseField = await directTaskService.CalculateDirectTaskAsync(mesh0, sensors, sources, primaryField);
        var eps0 = baseField.Select(f => f.Magnitude).ToArray();

        // 2. Строим якобиан на фоне один раз
        var J = await jacobianCacheService.BuildOnceAsync(mesh0, sensors, sources, eps0, primaryField);
        var matJ = Matrix<double>.Build.DenseOfArray(J);
        var JT = matJ.Transpose();
        var JTJ = JT * matJ;

        // 3. Регуляризация (Тихонов I порядка)
        for (int i = 0; i < n; i++)
            JTJ[i, i] += inversionOptions.Lambda;

        // 4. Итерации уточнения модели
        double[] eps_k = new double[m];
        double previousFunctional = double.MaxValue;

        for (int iter = 0; iter < inversionOptions.MaxIterations; iter++)
        {
            for (int i = 0; i < n; i++)
                initialMesh.Elements[i].Mu = mu[i];

            var epsField = await directTaskService.CalculateDirectTaskAsync(
                initialMesh,
                sensors,
                sources,
                primaryField
            );
            eps_k = epsField.Select(f => f.Magnitude).ToArray();

            double functional = 0.0;
            for (int i = 0; i < m; i++)
            {
                var r = observedValues[i] - eps_k[i];
                functional += r * r;
            }

            Console.WriteLine($"[Iter {iter}] Functional: {functional:E8}");

            if (Math.Abs(previousFunctional - functional) < inversionOptions.FunctionalThreshold)
            {
                Console.WriteLine("Stopping: functional change below tolerance.");
                break;
            }

            previousFunctional = functional;

            // Правая часть: eps_obs - eps0 (фиксировано)
            var residual = Vector<double>.Build.DenseOfEnumerable(observedValues.Zip(eps0, (obs, b0) => obs - b0));
            var JTr = JT * residual;

            // Решение: deltaMu
            var JTJLU = JTJ.LU();
            var delta = JTJLU.Solve(JTr);

            for (int i = 0; i < n; i++)
                mu[i] += delta[i];
        }

        return mu;
    }
}