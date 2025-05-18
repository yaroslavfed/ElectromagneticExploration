namespace Electromagnetic.Common.Models;

/// <summary>
/// Параметры для управления адаптивным уточнением и объединением ячеек сетки.
/// </summary>
public record MeshRefinementOptions
{
    /// <summary>
    /// Минимальная доля размера оси для ячеек (например, 0.1 = 10% от размера области).
    /// </summary>
    public double MinCellSizeFraction { get; init; } = 0.1;

    /// <summary>
    /// Максимальная доля размера оси для ячеек (например, 0.5 = 50% от размера области).
    /// </summary>
    public double MaxCellSizeFraction { get; init; } = 0.5;

    /// <summary>
    /// Максимальный уровень дробления ячейки.
    /// </summary>
    public int MaxSubdivisionLevel { get; init; } = 5;

    /// <summary>
    /// Начальный порог невязки для дробления ячейки.
    /// </summary>
    public double InitialRefineThreshold { get; init; } = 1e-4;

    /// <summary>
    /// Начальный порог невязки для объединения ячеек.
    /// </summary>
    public double InitialMergeThreshold { get; init; } = 1e-6;

    /// <summary>
    /// Коэффициент уменьшения порогов дробления и объединения на каждой итерации.
    /// </summary>
    public double RefinementDecay { get; init; } = 0.8;

    /// <summary>
    /// Уточнение: если функционал уменьшился в X раз.
    /// </summary>
    public double FunctionalImprovementRatio { get; init; } = 100.0;

    /// <summary>
    /// Уточнение: если абсолютное уменьшение функционала стало малым.
    /// </summary>
    public double AdaptiveTriggerTolerance { get; init; } = 1e-12;
}