namespace Electromagnetic.Common.Data.Domain;

public record CurrentSegment
{
    public required Point3D Center { get; init; }
    public required Vector3D Direction { get; init; } // Нормированный вектор направления
    public required double Current { get; init; }     // Величина тока в сегменте
}