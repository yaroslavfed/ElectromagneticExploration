namespace Electromagnetic.Common.Data.Domain;

/// <summary>
/// Структура конечного элемента с нумерованными узлами и ребрами
/// </summary>
public record FiniteElementWithNumerics
{
    /// <summary>
    /// Список узлов КЭ
    /// </summary>
    public IList<int> Nodes { get; } = [];

    /// <summary>
    /// Список ребер КЭ
    /// </summary>
    public IList<int> Edges { get; } = [];

    /// <summary>
    /// Соотношение ребер с их узлами
    /// </summary>
    public IList<(int First, int Second)> MapNodesEdges { get; } = [];
}