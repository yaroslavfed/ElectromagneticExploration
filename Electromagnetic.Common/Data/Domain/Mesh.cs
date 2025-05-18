namespace Electromagnetic.Common.Data.Domain;

/// <summary>
/// Параллелепипедальная сетка исследуемого объекта
/// </summary>
public record Mesh
{
    /// <summary>
    /// Список конечных элементов расчётной области
    /// </summary>
    public List<FiniteElement> Elements { get; init; } = [];

    /// <summary>
    /// Получаем ребро по его номеру
    /// </summary>
    /// <param name="index">Глобальный индекс ребра</param>
    /// <returns>Объект ребра</returns>
    public Edge GetEdgeByIndex(int index) =>
        (from element in Elements from edge in element.Edges where edge.EdgeIndex == index select edge).Single();

    public (double MinX, double MinY, double MinZ, double MaxX, double MaxY, double MaxZ) GetSizes()
    {
        var allX = from element in Elements from edge in element.Edges from node in edge.Nodes select node.Coordinate.X;
        var allY = from element in Elements from edge in element.Edges from node in edge.Nodes select node.Coordinate.Y;
        var allZ = from element in Elements from edge in element.Edges from node in edge.Nodes select node.Coordinate.Z;

        var xs = allX as double[] ?? allX.ToArray();
        var minX = xs.Min();
        var maxX = xs.Max();

        var ys = allY as double[] ?? allY.ToArray();
        var minY = ys.Min();
        var maxY = ys.Max();

        var zs = allZ as double[] ?? allZ.ToArray();
        var minZ = zs.Min();
        var maxZ = zs.Max();

        return (minX, minY, minZ, maxX, maxY, maxZ);
    }
}