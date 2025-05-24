namespace Electromagnetic.Common.Data.Domain;

public record Vector3D
{
    public Vector3D() { }

    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }

    public static Vector3D Zero => new(0.0, 0.0, 0.0);

    public double Norm() => Math.Sqrt(X * X + Y * Y + Z * Z);

    public Vector3D Normalize()
    {
        double norm = Norm();
        return norm == 0
            ? new Vector3D(0, 0, 0)
            : new Vector3D(X / norm, Y / norm, Z / norm);
    }

    public double Dot(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;

    public Vector3D Cross(Vector3D other) =>
        new(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y - Y * other.X);

    public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3D operator *(Vector3D v, double scalar) => new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3D operator *(double scalar, Vector3D v) => v * scalar;

    public static Vector3D operator /(Vector3D v, double scalar)
    {
        return new(v.X / scalar, v.Y / scalar, v.Z / scalar);
    }

    public override string ToString() => $"({X:0.###}, {Y:0.###}, {Z:0.###})";
}