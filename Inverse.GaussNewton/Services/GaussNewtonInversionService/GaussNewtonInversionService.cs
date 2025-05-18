using System.Diagnostics;
using System.Text.Json;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;
using Inverse.GaussNewton.Services.InverseService;
using Inverse.GaussNewton.Services.JacobianService;
using Inverse.SharedCore.DirectTaskService;

namespace Inverse.GaussNewton.Services.GaussNewtonInversionService;

public class GaussNewtonInversionService(
    IInversionService inversionService,
    IJacobianService jacobianService,
    IDirectTaskService directTaskService
) : IGaussNewtonInversionService
{
    private readonly Stopwatch _timer = new();
    private          double    _initialFunctional;

    /// <inheritdoc />
    public async Task AdaptiveInvertAsync(
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
        // Истинные значения
        var observedValues = trueModelValues.Select(s => s.Magnitude).ToArray();

        var currentMesh = initialMesh;
        double currentFunctional = .0;

        double previousFunctional = double.MaxValue;

        _timer.Start();
        for (var iteration = 0; iteration < inversionOptions.MaxIterations; iteration++)
        {
            Console.WriteLine($"\n== Adaptive Inversion Iteration {iteration + 1} ==");

            // Расчёт прямой задачи
            Console.WriteLine($"Calculating direct task on the iteration step [{iteration}]");
            var anomalySensors = await directTaskService.CalculateDirectTaskAsync(
                currentMesh,
                sensors,
                sources,
                primaryField
            );
            var modelValues = anomalySensors.Select(s => s.Magnitude).ToArray();

            // Расчёт невязки и вычисление функционала
            currentFunctional = .0;
            for (var i = 0; i < modelValues.Length; i++)
            {
                var residual = observedValues[i] - modelValues[i];
                var weight = 1; // TODO: заменить на применения весов

                currentFunctional += residual * residual * weight * weight;
            }

            Console.WriteLine($"Current functional [iteration {iteration}]: {currentFunctional:E8}");

            // Сохранение начального функционала
            if (iteration == 0)
            {
                _initialFunctional = currentFunctional;
                Console.WriteLine($"Initial functional was set to: {_initialFunctional:E8}");
            }

            // Проверка на нулевой функционал
            if (currentFunctional == 0)
            {
                Console.WriteLine("The model has true parameters");
                break;
            }

            // Проверка на достижение искомого функционала
            if (currentFunctional / _initialFunctional < inversionOptions.FunctionalThreshold)
            {
                Console.WriteLine("The desired value of the functional has been achieved");
                break;
            }

            // Проверка на стагнацию функционала
            if (iteration != 0)
            {
                var difference = previousFunctional - currentFunctional;
                Console.WriteLine($"Difference between previous functional and current functional: {difference:E8}");

                if (Math.Abs(difference) < inversionOptions.RelativeTolerance)
                {
                    Console.WriteLine("Small relative change in functional");
                }
            }

            previousFunctional = currentFunctional;

            // Построение A
            Console.WriteLine("Calculating jacobian was started");
            var jacobian = await jacobianService.BuildJacobianAsync(currentMesh, sensors, sources, primaryField);
            Console.WriteLine("Calculating jacobian was ended");

            // Текущие параметры модели
            var modelParameters = currentMesh.Elements.Select(c => c.Mu).ToArray();

            // Итерация метода Гаусса–Ньютона (Решение обратной задачи)
            var updatedMu = inversionService.Invert(
                currentMesh,
                modelValues,
                observedValues,
                jacobian,
                modelParameters, // текущие значения мю
                inversionOptions,
                iteration,
                out var effectiveLambda
            );

            // Лог состояния итерации
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(
                $"[Iteration {iteration + 1}] Functional: {currentFunctional:E8} | Lambda: {effectiveLambda:E5} | UseTikhonovFirstOrder: {inversionOptions.UseTikhonovFirstOrder} | UseTikhonovSecondOrder: {inversionOptions.UseTikhonovSecondOrder}"
            );
            Console.ResetColor();

            // Обновляем плотности ячеек
            for (int j = 0; j < currentMesh.Elements.Count; j++)
                currentMesh.Elements[j].Mu = updatedMu[j];
            Console.WriteLine($"Elements: {currentMesh.Elements.Count}");
        }

        _timer.Stop();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Elapsed time: {_timer.Elapsed}");
        Console.WriteLine($"Initial functional: {_initialFunctional:E8}\t|\tLast functional: {currentFunctional:E8}");
        Console.ResetColor();

        ShowPlotAsync(currentMesh);
    }

    private void ShowPlotAsync(Mesh mesh)
    {
        const string jsonFile = "inverse.json";
        const string pythonPath = "python";
        const string outputImage = "inverse.png";

        File.WriteAllText(jsonFile, JsonSerializer.Serialize(mesh));
        var currentDirectory = Directory.GetCurrentDirectory();
        var scriptPath = Path.Combine(currentDirectory, "Scripts\\inverse_chart.py");

        var psi = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"{scriptPath} {jsonFile} {outputImage}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        process?.WaitForExit();
    }
}