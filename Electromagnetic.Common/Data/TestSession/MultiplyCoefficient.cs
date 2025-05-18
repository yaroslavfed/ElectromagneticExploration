using Electromagnetic.Common.Data.Domain;

namespace Electromagnetic.Common.Data.TestSession;

/// <summary>
/// Кожффициент разрядки
/// </summary>
public record MultiplyCoefficient
{
    /// <summary>
    /// Коэффициент разрядки по трем осям
    /// </summary>
    public required Point3D Coefficient { get; init; }
}