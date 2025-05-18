namespace Electromagnetic.Common.Data.TestSession;

public record TestResult
{
    public Guid Id { get; init; }

    public IEnumerable<string> Plots { get; init; } = [];

    public int ItersCount { get; set; }

    public SolutionAdditionalInfo? SolutionInfo { get; set; }
}