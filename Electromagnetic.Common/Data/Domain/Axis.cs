using Electromagnetic.Common.Data.InputModels;

namespace Electromagnetic.Common.Data.Domain;

/// <summary>
/// Набор входных параметров построения сетки
/// </summary>
public record Axis
{
    /// <summary>
    /// Дополнительные параметры векторного МКЭ
    /// </summary>
    public required AdditionalParameters AdditionalParameters { get; init; }

    /// <summary>
    /// Параметры геометрической параметризации расчетной области
    /// </summary>
    public required Positioning Positioning { get; init; }

    /// <summary>
    /// Дробление сетки
    /// </summary>
    public required Splitting Splitting { get; init; }

    /// <summary>
    /// Список физических объектов
    /// </summary>
    public required IReadOnlyList<Strata> StrataList { get; init; }
}