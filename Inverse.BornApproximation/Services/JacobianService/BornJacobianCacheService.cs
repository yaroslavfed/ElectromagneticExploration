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
        IReadOnlyList<FieldSample> primaryField
    )
    {
        if (_cachedJacobian is
            { })
            return _cachedJacobian;

        var m = sensors.Count;
        var n = mesh.Elements.Count;
        var jacobian = new double[m, n];

        // 1. Базовое решение
        var baseValues = primaryField.Select(s => s.Magnitude).ToArray();

        // 2. Перебираем ячейки и рассчитываем якобиан по численной производной
        for (var j = 0; j < n; j++)
        {
            var perturbedMesh = CloneMeshWithPerturbedMu(mesh, j);
            var perturbedField = await directTaskService.CalculateDirectTaskAsync(
                perturbedMesh,
                sensors,
                sources,
                primaryField
            );

            var perturbedValues = perturbedField.Select(s => s.Magnitude).ToArray();

            var originalMu = mesh.Elements[j].Mu;
            var delta = 0.1 * Math.Max(1.0, Math.Abs(originalMu));

            for (var i = 0; i < m; i++)
            {
                jacobian[i, j] = (perturbedValues[i] - baseValues[i]) / delta;
            }
        }

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
        double delta = 0.1 * Math.Max(1.0, Math.Abs(originalMu));
        clonedElements[indexToPerturb].Mu += delta;

        return new Mesh { Elements = clonedElements };
    }
}