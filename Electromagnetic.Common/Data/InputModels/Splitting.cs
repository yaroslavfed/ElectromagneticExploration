using Electromagnetic.Common.Data.Domain;

namespace Electromagnetic.Common.Data.InputModels;

/// <summary>
/// Параметры дробления объекта
/// </summary>
public record Splitting
{
    /// <summary>
    /// Количество разбиений объекта
    /// </summary>
    public required Point3D SplittingCoefficient { get; init; }

    /// <summary>
    /// Коэффициент разрядки
    /// </summary>
    public required Point3D MultiplyCoefficient { get; init; }
}