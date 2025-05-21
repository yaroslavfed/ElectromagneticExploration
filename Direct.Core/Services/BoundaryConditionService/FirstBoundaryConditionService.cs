using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.BoundaryConditionService;

public class FirstBoundaryConditionService : IBoundaryConditionService
{
    public Task ApplyBoundaryConditionsAsync(Matrix matrix, Vector rhs, Mesh mesh, double eps = 1e-8)
    {
        // Предварительно получаем размеры области
        var (minX, minY, minZ, maxX, maxY, maxZ) = mesh.GetSizes();

        // Кэшируем граничные индексы, чтобы не делать обнуление матрицы многократно
        var boundaryEdgeIndices = new HashSet<int>();

        foreach (var element in mesh.Elements)
        {
            foreach (var edge in element.Edges)
            {
                var edgeIndex = edge.EdgeIndex;
                if (boundaryEdgeIndices.Contains(edgeIndex))
                    continue;

                // Если ребро лежит полностью на внешней границе
                bool isOnBoundary = edge.Nodes.All(n => Math.Abs(n.Coordinate.X - minX) < eps
                                                        || Math.Abs(n.Coordinate.X - maxX) < eps
                                                        || Math.Abs(n.Coordinate.Y - minY) < eps
                                                        || Math.Abs(n.Coordinate.Y - maxY) < eps
                                                        || Math.Abs(n.Coordinate.Z - minZ) < eps
                                                        || Math.Abs(n.Coordinate.Z - maxZ) < eps
                );

                if (isOnBoundary)
                {
                    int idx = edge.EdgeIndex;
                    boundaryEdgeIndices.Add(idx);
                    // Обнуляем соответствующий DOF
                    matrix.ClearRow(edgeIndex);
                    matrix.ClearColumn(edgeIndex);
                    matrix[edgeIndex, edgeIndex] = 1.0;
                    rhs[edgeIndex] = 0.0;
                }

                // bool isOnBoundary = false;
                // foreach (var node in edge.Nodes)
                // {
                //     var x = node.Coordinate.X;
                //     var y = node.Coordinate.Y;
                //     var z = node.Coordinate.Z;
                //
                //     if (Math.Abs(x - minX) < eps
                //         || Math.Abs(x - maxX) < eps
                //         || Math.Abs(y - minY) < eps
                //         || Math.Abs(y - maxY) < eps
                //         || Math.Abs(z - minZ) < eps
                //         || Math.Abs(z - maxZ) < eps)
                //     {
                //         isOnBoundary = true;
                //     }
                // }
                //
                // if (isOnBoundary)
                // {
                //     int idx = edge.EdgeIndex;
                //     boundaryEdgeIndices.Add(idx);
                //     matrix.ClearRow(idx);
                //     matrix.ClearColumn(idx);
                //     matrix[idx, idx] = 1.0;
                //     rhs[idx] = 0.0;
                // }
            }
        }

        return Task.CompletedTask;
    }
}