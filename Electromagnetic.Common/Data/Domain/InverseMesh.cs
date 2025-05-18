namespace Electromagnetic.Common.Data.Domain;

public record InverseMesh
{
    public IReadOnlyList<Cell> Cells { get; init; } = [];
}