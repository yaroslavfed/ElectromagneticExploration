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

    public async Task<double[,]> BuildAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        double[] currentValues, // Значения |B| на текущей итерации
        IReadOnlyList<FieldSample> primaryField
    )
    {
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

        double originalMu = clonedElements[indexToPerturb].Mu;
        double delta = ComputeDeltaMu(originalMu);
        clonedElements[indexToPerturb].Mu += delta;

        return new Mesh { Elements = clonedElements };
    }

    private static double ComputeDeltaMu(double mu) => 0.1 * Math.Max(1e-8, Math.Abs(mu));
}