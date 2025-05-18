namespace Electromagnetic.Common.Enums;

/// <summary>
/// Краевые условия
/// </summary>
public enum EBoundaryConditions
{
    /// <summary>
    /// Краевое условие первого рода
    /// </summary>
    Dirichlet,

    /// <summary>
    /// Краевое условие второго рода
    /// </summary>
    Neiman,

    /// <summary>
    /// Краевое условие третьего рода
    /// </summary>
    Robin
}