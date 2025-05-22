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
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        double[] currentValues, // Значения |B| на текущей итерации
        IReadOnlyList<FieldSample> primaryField
    )
    {
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
                    try
                    {
                        var perturbedMesh = CloneMeshWithPerturbedMu(mesh, localJ);
                        var perturbedField = await directTaskService.CalculateDirectTaskAsync(
                            perturbedMesh,
                            sensors,
                            sources
                        );
                        var perturbedValues = perturbedField.Select(s => s.Magnitude).ToArray();

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
        return jacobian;
    }

    private static Mesh CloneMeshWithPerturbedMu(Mesh originalMesh, int indexToPerturb)
    {
        var clonedElements = new List<FiniteElement>(originalMesh.Elements.Count);

        foreach (var originalElement in originalMesh.Elements)
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