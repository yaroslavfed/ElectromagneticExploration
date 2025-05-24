namespace Electromagnetic.Common.Data.Domain;

public record FieldSample
{
    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }

    public double Bx { get; init; }

    public double By { get; init; }

    public double Bz { get; init; }

    public double Magnitude => Math.Sqrt(Bx * Bx + By * By + Bz * Bz);
}