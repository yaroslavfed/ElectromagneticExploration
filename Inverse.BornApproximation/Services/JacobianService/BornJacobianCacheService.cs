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
    private double[,]? _cachedJacobian;

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
            return _cachedJacobian;

        int m = sensors.Count;
        int n = mesh.Elements.Count;
        var jacobian = new double[m, n];

        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);

        for (int j = 0; j < n; j++)
        {
            await semaphore.WaitAsync();
            int localJ = j;

            var task = Task.Run(async () =>
                {
                    try
                    {
                        var perturbedMesh = CloneMeshWithPerturbedMu(mesh, localJ);
                        var perturbedField = await directTaskService.CalculateDirectTaskAsync(
                            perturbedMesh,
                            sensors,
                            sources,
                            primaryField
                        );
                        var perturbedValues = perturbedField.Select(s => s.Magnitude).ToArray();

                        double originalMu = mesh.Elements[localJ].Mu;
                        double delta = ComputeDeltaMu(originalMu);

                        for (int i = 0; i < m; i++)
                        {
                            jacobian[i, localJ] = (perturbedValues[i] - currentValues[i]) / delta;
                        }
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

        double originalMu = clonedElements[indexToPerturb].Mu;
        double delta = ComputeDeltaMu(originalMu);
        clonedElements[indexToPerturb].Mu += delta;

        return new Mesh { Elements = clonedElements };
    }

    private static double ComputeDeltaMu(double mu) => 0.1 * Math.Max(1e-8, Math.Abs(mu));
}