using Electromagnetic.Common.Data.Domain;

namespace Electromagnetic.Common.Data.TestSession;

/// <summary>
/// Коэффициент дробления сетки
/// </summary>
public record SplittingCoefficient
{
    /// <summary>
    /// Коэффициент дробления сетки по трем осям
    /// </summary>
    public required Point3D Coefficient { get; init; }
}