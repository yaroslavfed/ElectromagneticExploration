using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.StaticServices.IntegrationHelper;

public static class IntegrationHelper
{
    public static List<IntegrationPoint> GetIntegrationPoints(FiniteElement element)
    {
        // Извлекаем уникальные координаты узлов без ToList
        var coords = new HashSet<Point3D>();

        foreach (var edge in element.Edges)
        {
            foreach (var node in edge.Nodes)
            {
                coords.Add(node.Coordinate); // предполагается, что Point3D реализует Equals + GetHashCode
            }
        }

        // Инициализируем экстремумы
        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;
        double minZ = double.MaxValue, maxZ = double.MinValue;

        foreach (var p in coords)
        {
            if (p.X < minX) minX = p.X;
            if (p.X > maxX) maxX = p.X;

            if (p.Y < minY) minY = p.Y;
            if (p.Y > maxY) maxY = p.Y;

            if (p.Z < minZ) minZ = p.Z;
            if (p.Z > maxZ) maxZ = p.Z;
        }

        var centerX = (minX + maxX) * 0.5;
        var centerY = (minY + maxY) * 0.5;
        var centerZ = (minZ + maxZ) * 0.5;

        // Предвыделяем список нужного размера
        var points = new List<IntegrationPoint>(8);
        var weight = element.Volume / 8.0;

        double[] xCoords = [minX, centerX];
        double[] yCoords = [minY, centerY];
        double[] zCoords = [minZ, centerZ];

        for (int xi = 0; xi < 2; xi++)
            for (int yi = 0; yi < 2; yi++)
                for (int zi = 0; zi < 2; zi++)
                {
                    points.Add(
                        new()
                        {
                            Position = new(xCoords[xi], yCoords[yi], zCoords[zi]), Weight = weight
                        }
                    );
                }

        return points;
    }
}