namespace Electromagnetic.Common.Data.Domain;

/// <summary>
/// Структура узла трехмерной сетки 
/// </summary>
public record Node
{
    /// <summary>
    /// Номер узла в конечном элементе
    /// </summary>
    public int NodeIndex { get; init; }

    /// <summary>
    /// Координаты узла в декартовой системе координат
    /// </summary>
    public required Point3D Coordinate { get; init; }
}