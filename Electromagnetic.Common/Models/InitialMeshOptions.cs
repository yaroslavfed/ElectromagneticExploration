namespace Electromagnetic.Common.Models;

public record InitialMeshOptions
{
    public int SplitsX { get; init; } = 15;

    public int SplitsY { get; init; } = 15;

    public int SplitsZ { get; init; } = 10;

    public double Depth { get; init; } = 10;
}