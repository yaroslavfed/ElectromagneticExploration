using Electromagnetic.Common.Enums;

namespace Electromagnetic.Common.Data.Domain;

public record Sensor
{
    public required Point3D Position { get; init; }

    public required ESensorComponent ComponentDirection { get; init; }
}