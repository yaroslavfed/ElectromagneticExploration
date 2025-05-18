namespace Electromagnetic.Common.Data.TestSession;

/// <summary>
/// Дополнительная информация о результате решения
/// </summary>
/// <remarks>
/// Включает в себя вектора полученных и истинных значений, а так же абсолютную и относительную погрешность
/// </remarks>
public record SolutionAdditionalInfo
{
    /// <summary>
    /// Номера векторов
    /// </summary>
    public IList<int> EdgeNumber { get; init; } = [];

    /// <summary>
    /// Вектор решения
    /// </summary>
    public IList<double> EdgeVectorValue { get; init; } = [];

    /// <summary>
    /// Вектор истинных значений
    /// </summary>
    public IList<double> EdgeVectorTruthValue { get; init; } = [];

    /// <summary>
    /// Абсолютная погрешность решения
    /// </summary>
    public IList<double> Inaccuracy { get; init; } = [];

    /// <summary>
    /// Относительная погрешность решения
    /// </summary>
    public double Discrepancy { get; init; }
}