using System.Collections.Concurrent;
using Electromagnetic.Common.Data.Domain;
using Inverse.SharedCore.DirectTaskService;

namespace Inverse.BornApproximation.Services.JacobianService;

/// <summary>
/// Сервис однократного расчёта якобиана для фоновой модели (метод Борна).
/// </summary>
public class BornJacobianCacheService(
    IDirectTaskService directTaskService
) : IBornJacobianCacheService
{
    private          double[,]?         _cachedJacobian;
    private readonly ConcurrentBag<int> _calculationProgress = [];

    public async Task<double[,]> BuildOnceAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        double[] currentValues,
        IReadOnlyList<FieldSample> primaryField
    )
    {
        if (_cachedJacobian is
            { })
        {
            Console.WriteLine("Jacobian cache is already cached");
            return _cachedJacobian;
        }

        _calculationProgress.Clear();

        var m = sensors.Count;
        var n = mesh.Elements.Count;
        var jacobian = new double[m, n];

        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);

        for (var j = 0; j < n; j++)
        {
            await semaphore.WaitAsync();
            var localJ = j;

            var task = Task.Run(async () =>
                {
                    Console.WriteLine($"Jacobian calculation started at the element {localJ}");
                    try
                    {
                        Console.WriteLine($"Cloning mesh started at the element {localJ}");
                        var perturbedMesh = CloneMeshWithPerturbedMu(mesh, localJ);
                        Console.WriteLine($"Cloning mesh finished at the element {localJ}");

                        Console.WriteLine($"Direct task started at the element {localJ}");
                        var perturbedField = await directTaskService.CalculateDirectTaskAsync(
                            perturbedMesh,
                            sensors,
                            sources,
                            primaryField
                        );
                        var perturbedValues = perturbedField.Select(s => s.Magnitude).ToArray();
                        Console.WriteLine($"Direct task finished at the element {localJ}");

                        var originalMu = mesh.Elements[localJ].Mu;
                        var delta = ComputeDeltaMu(originalMu);

                        for (var i = 0; i < m; i++)
                        {
                            jacobian[i, localJ] = (perturbedValues[i] - currentValues[i]) / delta;
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

        _cachedJacobian = jacobian;
        return jacobian;
    }

    private static Mesh CloneMeshWithPerturbedMu(Mesh mesh, int indexToPerturb)
    {
        var clonedElements = new List<FiniteElement>(mesh.Elements.Count);

        foreach (var originalElement in mesh.Elements)
        {
            var clonedEdges = originalElement
                              .Edges
                              .Select(edge => new Edge
                                  {
                                      EdgeIndex = edge.EdgeIndex,
                                      Nodes = edge
                                              .Nodes
                                              .Select(node => new Node
                                                  {
                                                      NodeIndex = node.NodeIndex,
                                                      Coordinate = new Point3D(
                                                          node.Coordinate.X,
                                                          node.Coordinate.Y,
                                                          node.Coordinate.Z
                                                      )
                                                  }
                                              )
                                              .ToList()
                                  }
                              )
                              .ToList();

            clonedElements.Add(new FiniteElement { Edges = clonedEdges, Mu = originalElement.Mu });
        }

        var originalMu = clonedElements[indexToPerturb].Mu;
        var delta = ComputeDeltaMu(originalMu);
        clonedElements[indexToPerturb].Mu += delta;

        return new Mesh { Elements = clonedElements };
    }

    private static double ComputeDeltaMu(double mu) => 0.1 * Math.Max(1e-8, Math.Abs(mu));
}