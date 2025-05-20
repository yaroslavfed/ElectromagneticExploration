using System.Diagnostics;
using System.Text.Json;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Extensions;
using Electromagnetic.Common.Models;
using Inverse.BornApproximation.Services.JacobianService;
using Inverse.SharedCore.DirectTaskService;
using MathNet.Numerics.LinearAlgebra;

// ReSharper disable InconsistentNaming

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
    private readonly Stopwatch _timer = new();
    private          double    _initialFunctional;

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

        var n = currentMesh.Elements.Count;
        var m = sensors.Count;
        var mu = Enumerable.Repeat(baseMu, n).ToArray();

        // Строим фоновую модель и решаем A0 (один раз)
        Console.WriteLine("Cloning base mesh started");
        var mesh0 = currentMesh.CloneWithUniformMu(baseMu);
        Console.WriteLine("Cloning base mesh finished");

        Console.WriteLine("Direct task started");
        var baseField = await directTaskService.CalculateDirectTaskAsync(mesh0, sensors, sources, primaryField);
        var eps0 = baseField.Select(f => f.Magnitude).ToArray();
        Console.WriteLine("Direct task finished");

        // Строим J на фоне один раз
        Console.WriteLine("Jacobian calculation started");
        var J = await jacobianCacheService.BuildOnceAsync(mesh0, sensors, sources, eps0, primaryField);
        Console.WriteLine("Jacobian calculation finished");
        var matJ = Matrix<double>.Build.DenseOfArray(J);
        var JT = matJ.Transpose();
        var JTJ = JT * matJ;

        // Регуляризация (Тихонов 1)
        for (int i = 0; i < n; i++)
            JTJ[i, i] += inversionOptions.Lambda;

        // Итерации для уточнения модели
        for (int iteration = 0; iteration < inversionOptions.MaxIterations; iteration++)
        {
            Console.WriteLine($"\n== Adaptive Inversion Iteration {iteration + 1} ==");

            // Обновляем плотности ячеек
            for (int j = 0; j < n; j++)
                currentMesh.Elements[j].Mu = mu[j];

            Console.WriteLine("Calculating direct task was started");
            var epsField = await directTaskService.CalculateDirectTaskAsync(
                currentMesh,
                sensors,
                sources,
                primaryField
            );
            var modelValues = epsField.Select(f => f.Magnitude).ToArray();
            Console.WriteLine("Calculating direct task was finished");

            // Расчёт невязки и вычисление функционала
            currentFunctional = .0;
            for (int i = 0; i < m; i++)
            {
                var r = trueValues[i] - modelValues[i];
                var weight = 1; // TODO: заменить на применения весов

                currentFunctional += r * r * weight * weight;
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

            // Правая часть: eps_obs - eps0
            var residual = Vector<double>.Build.DenseOfEnumerable(trueValues.Zip(eps0, (obs, b0) => obs - b0));
            var JTr = JT * residual;

            // Решение: deltaMu
            var JTJLU = JTJ.LU();
            var delta = JTJLU.Solve(JTr);

            for (int i = 0; i < n; i++)
                mu[i] += delta[i];

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