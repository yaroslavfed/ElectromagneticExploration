namespace Electromagnetic.Common.Data.Domain;

public record FieldSample
{
    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }

    public double Bx { get; set; }

    public double By { get; set; }

    public double Bz { get; set; }

    public double Magnitude { get; set; }
}