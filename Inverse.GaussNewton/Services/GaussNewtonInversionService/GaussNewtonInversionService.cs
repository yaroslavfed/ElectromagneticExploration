﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Direct.Core.Services.PlotService;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;
using Inverse.GaussNewton.Services.JacobianService;
using Inverse.SharedCore.Services.DirectTaskService;
using Inverse.SharedCore.Services.InverseService;

// ReSharper disable InconsistentNaming

namespace Inverse.GaussNewton.Services.GaussNewtonInversionService;

public class GaussNewtonInversionService(
    IInversionService inversionService,
    IGaussNewtonJacobianService gaussNewtonJacobianService,
    IDirectTaskService directTaskService,
    IPlotService plotService
) : IGaussNewtonInversionService
{
    private readonly Stopwatch                         _timer = new();
    private          double                            _initialFunctional;
    private          ConcurrentDictionary<int, double> _functionalList = [];

    /// <inheritdoc />
    public async Task AdaptiveInvertAsync(
        IReadOnlyList<FieldSample> trueModelValues,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<FieldSample> emptyValues,
        double baseMu,
        Mesh initialMesh,
        InverseOptions inversionOptions,
        MeshRefinementOptions refinementOptions
    )
    {
        // Запуск расчёта времени
        _timer.Start();

        // Истинные значения
        var currentMesh = initialMesh;

        double currentFunctional = .0;
        double previousFunctional = double.MaxValue;

        for (var iteration = 0; iteration < inversionOptions.MaxIterations; iteration++)
        {
            Console.WriteLine($"\n== Gauss-Newton inversion: iteration[{iteration + 1}] ==");

            // Расчёт прямой задачи
            var currentModelValues = await directTaskService.CalculateDirectTaskAsync(
                currentMesh,
                sensors,
                sources,
                emptyValues
            );

            // Расчёт невязки и вычисление функционала
            currentFunctional = .0;

            for (var i = 0; i < currentModelValues.Count; i++)
            {
                var dBx = currentModelValues[i].Bx - trueModelValues[i].Bx;
                var dBy = currentModelValues[i].By - trueModelValues[i].By;
                var dBz = currentModelValues[i].Bz - trueModelValues[i].Bz;

                currentFunctional += dBx * dBx + dBy * dBy + dBz * dBz;
            }

            // Сохранение начального функционала
            if (iteration == 0)
            {
                _initialFunctional = currentFunctional;
                Console.WriteLine($"Initial functional was set to: {_initialFunctional:E8}");

                _functionalList.TryAdd(-1, _initialFunctional);
            }

            // Проверка на нулевой функционал
            if (Math.Abs(currentFunctional) < 1e-18)
            {
                Console.WriteLine("The model has true parameters");
                break;
            }

            // Проверка на достижение искомого функционала
            var functionalDiv = currentFunctional / _initialFunctional;
            if (functionalDiv <= inversionOptions.FunctionalThreshold)
            {
                Console.WriteLine("The desired value of the functional has been achieved");
                break;
            }

            // Проверка на стагнацию функционала
            var iterationStagnation = false;
            if (iteration != 0)
            {
                var difference = previousFunctional - currentFunctional;
                if (difference < 0)
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Difference between previous functional and current functional: {difference:E8}");
                Console.ResetColor();

                inversionOptions.UseTikhonovSecondOrder = true;
                if (Math.Abs(difference) < inversionOptions.RelativeTolerance)
                {
                    Console.WriteLine("Small relative change in functional");
                    iterationStagnation = true;
                    inversionOptions.UseTikhonovSecondOrder = false;
                }
            }

            // Проверка на выход по времени
            if (inversionOptions.UseTimeThreshold && _timer.Elapsed.TotalMinutes >= inversionOptions.TimeThreshold)
            {
                Console.WriteLine("The time is out");
                break;
            }

            // Лог по итерации
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(
                $"Current functional: {currentFunctional:E8}\t|\tInitial functional: {_initialFunctional:E8}\n\r"
                + $"(Current functional) / (Initial functional): {functionalDiv}\t|\tFunctional threshold: {inversionOptions.FunctionalThreshold}\n\r"
                + $"Tikhonov first is enable: {inversionOptions.UseTikhonovFirstOrder}\tTikhonov second is enable: {inversionOptions.UseTikhonovSecondOrder}"
            );
            Console.ResetColor();

            previousFunctional = currentFunctional;

            // Построение A
            Console.WriteLine("Calculating jacobian was started");
            var timer = Stopwatch.StartNew();

            var matrixJ = await gaussNewtonJacobianService.BuildAsync(
                iteration,
                inversionOptions.MaxIterations,
                currentMesh,
                sensors,
                sources,
                currentModelValues,
                emptyValues
            );

            timer.Stop();
            Console.WriteLine($"Calculating jacobian was finished in time: {timer.Elapsed.TotalMinutes} minutes");

            // Текущие параметры модели
            var modelParameters = currentMesh.Elements.Select(c => c.Mu).ToArray();

            // Итерация метода Гаусса–Ньютона
            var observedValues = trueModelValues
                                 .SelectMany(v => new[]
                                     {
                                         v.Bx,
                                         v.By,
                                         v.Bz
                                     }
                                 )
                                 .ToArray();

            var modelValues = currentModelValues
                              .SelectMany(v => new[]
                                  {
                                      v.Bx,
                                      v.By,
                                      v.Bz
                                  }
                              )
                              .ToArray();

            var updatedMu = inversionService.Invert(
                currentMesh,
                modelValues,
                observedValues,
                matrixJ,
                modelParameters, // текущие значения mu
                inversionOptions,
                iteration,
                iterationStagnation
            );

            // Обновляем mu ячеек
            for (int j = 0; j < currentMesh.Elements.Count; j++)
                currentMesh.Elements[j].Mu = updatedMu[j];

            _functionalList.TryAdd(iteration, currentFunctional);
            Console.WriteLine($"Elements: {currentMesh.Elements.Count}");
        }

        _timer.Stop();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Elapsed time: {_timer.Elapsed}");
        Console.WriteLine($"Initial functional: {_initialFunctional:E8}\t|\tLast functional: {currentFunctional:E8}");
        Console.ResetColor();

        await WriteFunctionalToFile(currentFunctional);

        await ShowPlotAsync(currentMesh, sensors);
        var values = await directTaskService.CalculateDirectTaskAsync(currentMesh, sensors, sources, emptyValues);
        await ShowValuesAsync(values);
    }

    private async Task WriteFunctionalToFile(double lastFunctional)
    {
        await using var writer = new StreamWriter($"GaussNewton_test_3_{DateTime.Now.ToShortDateString()}.txt", false);

        await writer.WriteLineAsync($"Elapsed time: {_timer.Elapsed}");
        await writer.WriteLineAsync(
            $"Initial functional: {_initialFunctional:E8}\t|\tLast functional: {lastFunctional:E8}"
        );

        await writer.WriteLineAsync("\nFunctional list:");
        foreach (var functional in _functionalList)
            await writer.WriteLineAsync($"{functional.Key}: {functional.Value:E8}");
    }

    private async Task ShowPlotAsync(Mesh mesh, IReadOnlyList<Sensor> sensors)
    {
        await plotService.ShowPlotAsync(mesh, sensors);
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