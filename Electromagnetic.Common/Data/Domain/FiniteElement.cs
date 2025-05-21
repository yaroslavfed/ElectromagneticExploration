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

    private (double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ)? _cachedBounds;

    /// <summary>
    /// Объём КЭ
    /// </summary>
    public double Volume
    {
        get
        {
            var (minX, maxX, minY, maxY, minZ, maxZ) = GetBounds();
            return (maxX - minX) * (maxY - minY) * (maxZ - minZ);
        }
    }

    public Point3D GetCenter()
    {
        var (minX, maxX, minY, maxY, minZ, maxZ) = GetBounds();
        return new Point3D { X = (minX + maxX) / 2.0, Y = (minY + maxY) / 2.0, Z = (minZ + maxZ) / 2.0 };
    }

    public bool Contains(Point3D point, double epsilon = 1e-8)
    {
        var (minX, maxX, minY, maxY, minZ, maxZ) = GetBounds();
        return point.X >= minX - epsilon
               && point.X <= maxX + epsilon
               && point.Y >= minY - epsilon
               && point.Y <= maxY + epsilon
               && point.Z >= minZ - epsilon
               && point.Z <= maxZ + epsilon;
    }

    public (double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ) GetBounds()
    {
        if (_cachedBounds.HasValue)
            return _cachedBounds.Value;

        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;
        double minZ = double.MaxValue, maxZ = double.MinValue;

        var visited = new HashSet<int>();

        foreach (var edge in Edges)
        {
            foreach (var node in edge.Nodes)
            {
                if (!visited.Add(node.NodeIndex)) continue;

                var coord = node.Coordinate;

                if (coord.X < minX) minX = coord.X;
                if (coord.X > maxX) maxX = coord.X;

                if (coord.Y < minY) minY = coord.Y;
                if (coord.Y > maxY) maxY = coord.Y;

                if (coord.Z < minZ) minZ = coord.Z;
                if (coord.Z > maxZ) maxZ = coord.Z;
            }
        }

        _cachedBounds = (minX, maxX, minY, maxY, minZ, maxZ);
        return _cachedBounds.Value;
    }
}