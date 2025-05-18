using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.BoundaryConditionService;

public class FirstBoundaryConditionService : IBoundaryConditionService
{
    public Task ApplyBoundaryConditionsAsync(Matrix matrix, Vector rhs, Mesh mesh, double eps = 1e-8)
    {
        foreach (var element in mesh.Elements)
        {
            foreach (var edge in element.Edges)
            {
                var edgeIndex = edge.EdgeIndex;

                var p1 = edge.Nodes[0].Coordinate;
                var p2 = edge.Nodes[1].Coordinate;
                var d = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z).Normalize();

                // Если ребро лежит полностью на внешней границе
                bool isOnBoundary = edge.Nodes.All(n => Math.Abs(n.Coordinate.X - mesh.GetSizes().MinX) < eps
                                                        || Math.Abs(n.Coordinate.X - mesh.GetSizes().MaxX) < eps
                                                        || Math.Abs(n.Coordinate.Y - mesh.GetSizes().MinY) < eps
                                                        || Math.Abs(n.Coordinate.Y - mesh.GetSizes().MaxY) < eps
                                                        || Math.Abs(n.Coordinate.Z - mesh.GetSizes().MinZ) < eps
                                                        || Math.Abs(n.Coordinate.Z - mesh.GetSizes().MaxZ) < eps
                );

                if (isOnBoundary)
                {
                    // Обнуляем соответствующий DOF
                    matrix.ClearRow(edgeIndex);
                    matrix.ClearColumn(edgeIndex);
                    matrix[edgeIndex, edgeIndex] = 1.0;
                    rhs[edgeIndex] = 0.0;
                }
            }
        }

        return Task.CompletedTask;
    }

}