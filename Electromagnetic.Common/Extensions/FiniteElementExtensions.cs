using Electromagnetic.Common.Data.Domain;

namespace Electromagnetic.Common.Extensions;

public static class FiniteElementExtensions
{
    public static Point3D GetSizes(this FiniteElement element)
    {
        var nodes = element.Edges.SelectMany(edge => edge.Nodes).ToList();

        if (nodes.Count == 0)
            return new() { X = 0, Y = 0, Z = 0 };

        var minX = nodes.Min(n => n.Coordinate.X);
        var maxX = nodes.Max(n => n.Coordinate.X);
        var minY = nodes.Min(n => n.Coordinate.Y);
        var maxY = nodes.Max(n => n.Coordinate.Y);
        var minZ = nodes.Min(n => n.Coordinate.Z);
        var maxZ = nodes.Max(n => n.Coordinate.Z);

        return new() { X = maxX - minX, Y = maxY - minY, Z = maxZ - minZ };
    }
    
    public static int[] GetGlobalEdgeIndices(this FiniteElement element)
    {
        return element.Edges
                      .OrderBy(e => e.EdgeIndex) // Порядок должен совпадать с порядком в локальных матрицах
                      .Select(e => e.EdgeIndex)
                      .ToArray();
    }
}