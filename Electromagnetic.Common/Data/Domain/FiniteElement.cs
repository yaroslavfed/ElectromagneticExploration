using Electromagnetic.Common.Extensions;

namespace Electromagnetic.Common.Data.Domain;

/// <summary>
/// Параллелепипедальный КЭ векторного МКЭ
/// </summary>
public record FiniteElement
{
    /// <summary>
    /// Список ребер принадлежащих КЭ
    /// </summary>
    public IList<Edge> Edges { get; init; } = new List<Edge>();

    /// <summary>
    /// Плотность среды
    /// </summary>
    public double Mu { get; set; }

    /// <summary>
    /// Объем КЭ
    /// </summary>
    public double Volume => this.GetSizes().X * this.GetSizes().Y * this.GetSizes().Z;

    public Point3D GetCenter()
    {
        // Собираем все уникальные узлы, чтобы найти границы
        var nodes = Edges.SelectMany(e => e.Nodes).DistinctBy(n => n.NodeIndex).ToList();

        var minX = nodes.Min(n => n.Coordinate.X);
        var maxX = nodes.Max(n => n.Coordinate.X);

        var minY = nodes.Min(n => n.Coordinate.Y);
        var maxY = nodes.Max(n => n.Coordinate.Y);

        var minZ = nodes.Min(n => n.Coordinate.Z);
        var maxZ = nodes.Max(n => n.Coordinate.Z);

        return new() { X = (minX + maxX) / 2.0, Y = (minY + maxY) / 2.0, Z = (minZ + maxZ) / 2.0 };
    }

    public bool Contains(Point3D point, double epsilon = 1e-8)
    {
        var nodes = Edges.SelectMany(e => e.Nodes).DistinctBy(n => n.NodeIndex).ToList();

        var minX = nodes.Min(n => n.Coordinate.X);
        var maxX = nodes.Max(n => n.Coordinate.X);

        var minY = nodes.Min(n => n.Coordinate.Y);
        var maxY = nodes.Max(n => n.Coordinate.Y);

        var minZ = nodes.Min(n => n.Coordinate.Z);
        var maxZ = nodes.Max(n => n.Coordinate.Z);

        return point.X >= minX - epsilon
               && point.X <= maxX + epsilon
               && point.Y >= minY - epsilon
               && point.Y <= maxY + epsilon
               && point.Z >= minZ - epsilon
               && point.Z <= maxZ + epsilon;
    }

    public (double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ) GetBounds()
    {
        var pointsX = from edges in Edges from nodes in edges.Nodes select nodes.Coordinate.X;
        var pointsY = from edges in Edges from nodes in edges.Nodes select nodes.Coordinate.Y;
        var pointsZ = from edges in Edges from nodes in edges.Nodes select nodes.Coordinate.Z;

        var minX = pointsX.Min();
        var maxX = pointsX.Max();

        var minY = pointsY.Min();
        var maxY = pointsY.Max();

        var minZ = pointsZ.Min();
        var maxZ = pointsZ.Max();

        return (minX, maxX, minY, maxY, minZ, maxZ);
    }
}