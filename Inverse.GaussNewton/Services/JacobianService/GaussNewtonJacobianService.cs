using System.Collections.Concurrent;
using Electromagnetic.Common.Data.Domain;
using Inverse.SharedCore.DirectTaskService;

namespace Inverse.GaussNewton.Services.JacobianService;

/// <summary>
/// Сервис расчёта матрицы Якобиана для сетки ячеек и списка сенсоров.
/// </summary>
public class GaussNewtonJacobianService(
    IDirectTaskService directTaskService
) : IGaussNewtonJacobianService
{
    private readonly ConcurrentBag<int> _calculationProgress = [];

    public async Task<double[,]> BuildAsync(
        int iteration,
        int maxIterations,
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<FieldSample> currentModelValues,
        IReadOnlyList<FieldSample> primaryField
    )
    {
        _calculationProgress.Clear();

        var m = sensors.Count;
        var n = mesh.Elements.Count;
        var jacobian = new double[3 * m, n];

        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);

        for (var j = 0; j < n; j++)
        {
            await semaphore.WaitAsync();
            var localJ = j;

            var task = Task.Run(async () =>
                {
                    try
                    {
                        var perturbedMesh = CloneMeshWithPerturbedMu(mesh, localJ, iteration, maxIterations);
                        var perturbedField = await directTaskService.CalculateDirectTaskAsync(
                            perturbedMesh,
                            sensors,
                            sources,
                            primaryField
                        );
                        // var perturbedValues = perturbedField.Select(s => s.Magnitude).ToArray();

                        var originalMu = mesh.Elements[localJ].Mu;
                        var delta = ComputeDeltaMu(originalMu, iteration, maxIterations);

                        for (var i = 0; i < m; i++)
                        {
                            jacobian[3 * i + 0, localJ] = (perturbedField[i].Bx - currentModelValues[i].Bx) / delta;
                            jacobian[3 * i + 1, localJ] = (perturbedField[i].By - currentModelValues[i].By) / delta;
                            jacobian[3 * i + 2, localJ] = (perturbedField[i].Bz - currentModelValues[i].Bz) / delta;
                        }

                        _calculationProgress.Add(localJ);
                        Console.WriteLine($"Jacobian calculation is filled {_calculationProgress.Count}/{n}");
                    } finally
                    {
                        semaphore.Release();
                    }
                }
            );

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        semaphore.Dispose();
        return jacobian;
    }

    private static Mesh CloneMeshWithPerturbedMu(
        Mesh originalMesh,
        int indexToPerturb,
        int iteration,
        int maxIterations
    )
    {
        var elements = originalMesh
                       .Elements
                       .Select((e, i) => new FiniteElement
                           {
                               Edges = e.Edges,
                               Mu = i == indexToPerturb
                                   ? e.Mu + ComputeDeltaMu(e.Mu, iteration, maxIterations)
                                   : e.Mu
                           }
                       )
                       .ToList();

        return new() { Elements = elements };
    }

    private static double ComputeDeltaMu(double mu, int iteration, int maxIterations)
    {
        const double maxRelative = 2e-1;
        const double minRelative = 1e-3;

        double t = (double)iteration / maxIterations;
        double relative = maxRelative * (1.0 - t) + minRelative * t;

        double delta = relative * Math.Abs(mu);
        return Math.Max(delta, 1e-10);
    }
}