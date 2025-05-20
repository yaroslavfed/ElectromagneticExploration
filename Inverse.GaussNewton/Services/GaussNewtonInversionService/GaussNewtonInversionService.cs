using System.Diagnostics;
using System.Text.Json;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;
using Inverse.GaussNewton.Services.InverseService;
using Inverse.GaussNewton.Services.JacobianService;
using Inverse.SharedCore.DirectTaskService;

// ReSharper disable InconsistentNaming

namespace Inverse.GaussNewton.Services.GaussNewtonInversionService;

public class GaussNewtonInversionService(
    IInversionService inversionService,
    IGaussNewtonJacobianService gaussNewtonJacobianService,
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
        // Запуск расчёта времени
        _timer.Start();

        // Истинные значения
        var trueValues = trueModelValues.Select(s => s.Magnitude).ToArray();
        var currentMesh = initialMesh;

        double currentFunctional = .0;
        double previousFunctional = double.MaxValue;

        for (var iteration = 0; iteration < inversionOptions.MaxIterations; iteration++)
        {
            Console.WriteLine($"\n== Gauss-Newton inversion: iteration[{iteration + 1}] ==");

            // Расчёт прямой задачи
            Console.WriteLine("Calculating direct task was started");
            var anomalySensors = await directTaskService.CalculateDirectTaskAsync(
                currentMesh,
                sensors,
                sources,
                primaryField
            );
            var modelValues = anomalySensors.Select(s => s.Magnitude).ToArray();
            Console.WriteLine("Calculating direct task was finished");

            // Расчёт невязки и вычисление функционала
            currentFunctional = .0;
            for (var i = 0; i < modelValues.Length; i++)
            {
                var residual = trueValues[i] - modelValues[i];
                var weight = 1; // TODO: заменить на применения весов

                currentFunctional += residual * residual * weight * weight;
            }

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
            var functionalDiv = currentFunctional / _initialFunctional;

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(
                $"Current functional: {currentFunctional:E8}\t|\tInitial functional: {_initialFunctional:E8}\n "
                + $"(Current functional) / (Initial functional): {functionalDiv}\t|\tFunctional threshold: {inversionOptions.FunctionalThreshold}"
            );
            Console.ResetColor();

            if (functionalDiv <= inversionOptions.FunctionalThreshold)
            {
                Console.WriteLine("The desired value of the functional has been achieved");
                break;
            }

            // Проверка на стагнацию функционала
            if (iteration != 0)
            {
                var difference = previousFunctional - currentFunctional;
                if (difference < 0)
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Difference between previous functional and current functional: {difference:E8}");
                Console.ResetColor();

                if (Math.Abs(difference) < inversionOptions.RelativeTolerance)
                {
                    Console.WriteLine("Small relative change in functional");
                }
            }

            // Проверка на выход по времени
            if (inversionOptions.UseTimeThreshold && _timer.Elapsed.TotalMinutes >= inversionOptions.TimeThreshold)
            {
                Console.WriteLine("The time is out");
                break;
            }

            previousFunctional = currentFunctional;

            // Построение A
            Console.WriteLine("Calculating jacobian was started");
            var matrixJ = await gaussNewtonJacobianService.BuildAsync(
                currentMesh,
                sensors,
                sources,
                modelValues,
                primaryField
            );
            Console.WriteLine("Calculating jacobian was finished");

            // Текущие параметры модели
            var modelParameters = currentMesh.Elements.Select(c => c.Mu).ToArray();

            // Итерация метода Гаусса–Ньютона (Решение обратной задачи)
            var updatedMu = inversionService.Invert(
                currentMesh,
                modelValues,
                trueValues,
                matrixJ,
                modelParameters, // текущие значения мю
                inversionOptions,
                iteration
            );

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

        await ShowPlotAsync(currentMesh);

        var values = await directTaskService.CalculateDirectTaskAsync(currentMesh, sensors, sources, primaryField);
        await ShowValuesAsync(values);
    }

    private async Task ShowPlotAsync(Mesh mesh)
    {
        const string jsonFile = "inverse.json";
        const string pythonPath = "python";
        const string outputImage = "inverse.png";

        await File.WriteAllTextAsync(jsonFile, JsonSerializer.Serialize(mesh));
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
        await process?.WaitForExitAsync()!;
    }

    private async Task ShowValuesAsync(IReadOnlyList<FieldSample> values)
    {
        var json = JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("field_data.json", json);

        var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts\\contour_plot.py");
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = scriptPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = false,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        await process?.WaitForExitAsync()!;
    }
}