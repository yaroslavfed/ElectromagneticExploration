namespace Electromagnetic.Common.Data.Domain;

/// <summary>
/// Координаты в декартовом пространстве
/// </summary>
public record Point3D
{
    public Point3D() { }

    public Point3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Координата по X
    /// </summary>
    public double X { get; init; }

    /// <summary>
    /// Координата по Y
    /// </summary>
    public double Y { get; init; }

    /// <summary>
    /// Координата по Z
    /// </summary>
    public double Z { get; init; }

    public override string ToString() => $"[{X}, {Y}, {Z}]";
}