using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Enums;

namespace Direct.Core.Services.SourceProvider;

public class CurrentSourceProvider : ICurrentSourceProvider
{
    public Task<IReadOnlyList<CurrentSegment>> GetSourcesAsync(CurrentSource source)
    {
        return source.Type switch
        {
            ESourceType.Linear => Task.FromResult(GenerateLinearSource(source)),
            ESourceType.Loop => Task.FromResult(GenerateLoopSource(source)),
            _ => throw new NotImplementedException($"Unsupported source type: {source.Type}")
        };
    }

    private IReadOnlyList<CurrentSegment> GenerateLinearSource(CurrentSource source)
    {
        var segments = new List<CurrentSegment>();

        var dx = (source.End!.X - source.Start!.X) / source.SegmentsPerSide;
        var dy = (source.End.Y - source.Start.Y) / source.SegmentsPerSide;
        var dz = (source.End.Z - source.Start.Z) / source.SegmentsPerSide;

        var direction = new Vector3D(dx, dy, dz).Normalize();
        var currentPerSegment = source.Amperage / source.SegmentsPerSide;

        for (int i = 0; i < source.SegmentsPerSide; i++)
        {
            var x0 = source.Start.X + i * dx;
            var y0 = source.Start.Y + i * dy;
            var z0 = source.Start.Z + i * dz;

            var x1 = source.Start.X + (i + 1) * dx;
            var y1 = source.Start.Y + (i + 1) * dy;
            var z1 = source.Start.Z + (i + 1) * dz;

            var center = new Point3D((x0 + x1) / 2.0, (y0 + y1) / 2.0, (z0 + z1) / 2.0);
            segments.Add(new CurrentSegment { Center = center, Direction = direction, Current = currentPerSegment });
        }

        return segments;
    }

    private IReadOnlyList<CurrentSegment> GenerateLoopSource(CurrentSource source)
    {
        var segments = new List<CurrentSegment>();

        var center = source.Center!;
        var halfWidth = source.Width / 2.0;
        var halfHeight = source.Height / 2.0;

        var totalSegments = source.SegmentsPerSide * 4;
        var currentPerSegment = source.Amperage / totalSegments;

        Point3D[] corners = source.Plane switch
        {
            ELoopPlane.XY => [
                new(center.X - halfWidth, center.Y - halfHeight, center.Z),
                new(center.X + halfWidth, center.Y - halfHeight, center.Z),
                new(center.X + halfWidth, center.Y + halfHeight, center.Z),
                new(center.X - halfWidth, center.Y + halfHeight, center.Z)
            ],
            ELoopPlane.XZ => [
                new(center.X - halfWidth, center.Y, center.Z - halfHeight),
                new(center.X + halfWidth, center.Y, center.Z - halfHeight),
                new(center.X + halfWidth, center.Y, center.Z + halfHeight),
                new(center.X - halfWidth, center.Y, center.Z + halfHeight)
            ],
            ELoopPlane.YZ => [
                new(center.X, center.Y - halfWidth, center.Z - halfHeight),
                new(center.X, center.Y + halfWidth, center.Z - halfHeight),
                new(center.X, center.Y + halfWidth, center.Z + halfHeight),
                new(center.X, center.Y - halfWidth, center.Z + halfHeight)
            ],
            _ => throw new NotImplementedException($"Unsupported plane: {source.Plane}")
        };

        for (int side = 0; side < 4; side++)
        {
            var p0 = corners[side];
            var p1 = corners[(side + 1) % 4];

            var dx = (p1.X - p0.X) / source.SegmentsPerSide;
            var dy = (p1.Y - p0.Y) / source.SegmentsPerSide;
            var dz = (p1.Z - p0.Z) / source.SegmentsPerSide;

            var direction = new Vector3D(dx, dy, dz).Normalize();

            for (int i = 0; i < source.SegmentsPerSide; i++)
            {
                var x0 = p0.X + i * dx;
                var y0 = p0.Y + i * dy;
                var z0 = p0.Z + i * dz;

                var x1 = p0.X + (i + 1) * dx;
                var y1 = p0.Y + (i + 1) * dy;
                var z1 = p0.Z + (i + 1) * dz;

                var centerPoint = new Point3D((x0 + x1) / 2.0, (y0 + y1) / 2.0, (z0 + z1) / 2.0);
                segments.Add(new CurrentSegment { Center = centerPoint, Direction = direction, Current = currentPerSegment });
            }
        }

        return segments;
    }
}