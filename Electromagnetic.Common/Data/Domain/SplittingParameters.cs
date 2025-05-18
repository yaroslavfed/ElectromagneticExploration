namespace Electromagnetic.Common.Data.Domain;

public record SplittingParameters
{
    public double XSplittingCoefficient { get; init; }

    public double YSplittingCoefficient { get; init; }

    public double ZSplittingCoefficient { get; init; }

    public double XMultiplyCoefficient { get; init; }

    public double YMultiplyCoefficient { get; init; }

    public double ZMultiplyCoefficient { get; init; }
}