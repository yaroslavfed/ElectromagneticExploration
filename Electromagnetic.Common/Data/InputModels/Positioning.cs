using Electromagnetic.Common.Data.Domain;

namespace Electromagnetic.Common.Data.InputModels;

/// <summary>
/// Набор параметров позиционирования объекта
/// </summary>
public record Positioning
{
    /// <summary>
    /// Координата центра объекта в декартовой системе координат
    /// </summary>
    public required Point3D CenterCoordinate { get; init; }

    /// <summary>
    /// Расстояние от центра объекта до его границ
    /// </summary>
    public required Point3D BoundsDistance { get; init; }
}