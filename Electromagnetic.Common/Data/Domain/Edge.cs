namespace Electromagnetic.Common.Data.Domain;

/// <summary>
/// Структура ребра конечного элемента
/// </summary>
public record Edge
{
    /// <summary>
    /// Глобальный номер ребра
    /// </summary>
    public int EdgeIndex { get; init; }

    /// <summary>
    /// Узлы принадлежащие ребру
    /// </summary>
    public IList<Node> Nodes { get; init; } = [];
}