using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.StaticServices.IntegrationHelper;

public static class IntegrationHelper
{
    public static List<IntegrationPoint> GetIntegrationPoints(FiniteElement element)
    {
        var nodes = element.Edges.SelectMany(e => e.Nodes).DistinctBy(n => n.NodeIndex).ToList();

        var minX = nodes.Min(n => n.Coordinate.X);
        var maxX = nodes.Max(n => n.Coordinate.X);
        var minY = nodes.Min(n => n.Coordinate.Y);
        var maxY = nodes.Max(n => n.Coordinate.Y);
        var minZ = nodes.Min(n => n.Coordinate.Z);
        var maxZ = nodes.Max(n => n.Coordinate.Z);

        var centerX = (minX + maxX) / 2.0;
        var centerY = (minY + maxY) / 2.0;
        var centerZ = (minZ + maxZ) / 2.0;

        var points = new List<IntegrationPoint>();
        var weight = element.Volume / 8.0;

        foreach (var x in new[]
        {
            minX,
            centerX
        })
            foreach (var y in new[]
            {
                minY,
                centerY
            })
                foreach (var z in new[]
                {
                    minZ,
                    centerZ
                })
                {
                    points.Add(new() { Position = new(x, y, z), Weight = weight });
                }

        return points;
    }
}