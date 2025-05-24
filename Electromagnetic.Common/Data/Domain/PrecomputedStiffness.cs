using MathNet.Numerics.LinearAlgebra;

namespace Electromagnetic.Common.Data.Domain;

public record PrecomputedStiffness
{
    public Matrix GlobalMatrix { get; init; } = null!;

    public Matrix<double> GlobalMathNetMatrix => GlobalMatrix.ToMathNet();
}