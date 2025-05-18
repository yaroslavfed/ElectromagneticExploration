using Electromagnetic.Common.Enums;

namespace Electromagnetic.Common.Data.TestSession;

/// <summary>
/// Параметры сессии решения прямой задачи
/// </summary>
public record TestSession<TMesh>
{
    /// <summary>
    /// Расчётная область
    /// </summary>
    public required TMesh Mesh { get; init; }

    public double Gamma { get; init; }

    /// <summary>
    /// Краевое условие
    /// </summary>
    public EBoundaryConditions BoundaryCondition { get; init; }
}