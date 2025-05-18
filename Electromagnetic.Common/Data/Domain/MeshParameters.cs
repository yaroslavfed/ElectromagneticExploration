namespace Electromagnetic.Common.Data.Domain;

public record MeshParameters
{
    public double XCenterCoordinate { get; init; }

    public double YCenterCoordinate { get; init; }

    public double ZCenterCoordinate { get; init; }

    public double XStepToBounds { get; init; }

    public double YStepToBounds { get; init; }

    public double ZStepToBounds { get; init; }
}
