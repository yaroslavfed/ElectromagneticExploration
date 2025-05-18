using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.SourceProvider;

public class CurrentSourceProvider : ICurrentSourceProvider
{
    public Task<IReadOnlyList<CurrentSegment>> GetSourcesAsync(CurrentSource source)
    {
        var segments = new List<CurrentSegment>();

        var dx = (source.End.X - source.Start.X) / source.Segments;
        var dy = (source.End.Y - source.Start.Y) / source.Segments;
        var dz = (source.End.Z - source.Start.Z) / source.Segments;

        var segmentLength = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        var direction = new Vector3D(dx, dy, dz).Normalize();

        var currentPerSegment = source.Amperage / source.Segments;

        for (int i = 0; i < source.Segments; i++)
        {
            var x0 = source.Start.X + i * dx;
            var y0 = source.Start.Y + i * dy;
            var z0 = source.Start.Z + i * dz;

            var x1 = source.Start.X + (i + 1) * dx;
            var y1 = source.Start.Y + (i + 1) * dy;
            var z1 = source.Start.Z + (i + 1) * dz;

            var center = new Point3D { X = (x0 + x1) / 2.0, Y = (y0 + y1) / 2.0, Z = (z0 + z1) / 2.0 };

            segments.Add(new() { Center = center, Direction = direction, Current = currentPerSegment });
        }

        return Task.FromResult<IReadOnlyList<CurrentSegment>>(segments);
    }
}