using System.Diagnostics;
using Electromagnetic.Common.Data.Domain;

namespace Inverse.SharedCore.DirectTaskService;

public class DirectTaskService(
    Direct.Core.Services.DirectTaskService.IDirectTaskService directTaskService
) : IDirectTaskService
{
    public async Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<FieldSample> primaryField
    )
    {
        var timer = Stopwatch.StartNew();
        var result = await directTaskService.CalculateDirectTaskAsync(mesh, sensors, sources, primaryField);
        timer.Stop();
        Console.WriteLine($"Calculation direct task was finished in {timer.Elapsed.TotalMinutes}m");
        return result;
    }

    public async Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        TestSession testSessionParameters,
        IReadOnlyList<FieldSample> primaryField
    )
    {
        var timer = Stopwatch.StartNew();
        var result = await directTaskService.CalculateDirectTaskAsync(testSessionParameters, primaryField);
        timer.Stop();
        Console.WriteLine($"Calculation direct task was finished in {timer.Elapsed.TotalMinutes}m");
        return result;
    }

    public async Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources
    )
    {
        var timer = Stopwatch.StartNew();
        var result = await directTaskService.CalculateDirectTaskAsync(mesh, sensors, sources);
        timer.Stop();
        Console.WriteLine($"Calculation direct task was finished in {timer.Elapsed.TotalMinutes}m");
        return result;
    }

    public async Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(TestSession testSessionParameters)
    {
        var timer = Stopwatch.StartNew();
        var result = await directTaskService.CalculateDirectTaskAsync(testSessionParameters);
        timer.Stop();
        Console.WriteLine($"Calculation direct task was finished in {timer.Elapsed.TotalMinutes}m");
        return result;
    }
}