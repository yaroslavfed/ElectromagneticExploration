namespace Electromagnetic.Common.Data.Domain;

public record TestSession
{
    public required MeshParameters MeshParameters { get; init; }

    public required SplittingParameters SplittingParameters { get; init; }

    public required AdditionParameters AdditionParameters { get; init; }

    public required IReadOnlyList<Strata> StrataList { get; init; }

    public required CurrentSource CurrentSource { get; init; }

    public required IReadOnlyList<Sensor> Sensors { get; set; }
}