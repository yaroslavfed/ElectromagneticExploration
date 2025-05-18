using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.StaticServices.IntegrationHelper;

public record IntegrationPoint
{
    public required Point3D Position { get; init; }

    public required double Weight { get; init; }
}