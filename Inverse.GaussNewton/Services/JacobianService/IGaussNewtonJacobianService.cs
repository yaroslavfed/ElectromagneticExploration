using Electromagnetic.Common.Data.Domain;

namespace Inverse.GaussNewton.Services.JacobianService;

public interface IGaussNewtonJacobianService
{
    Task<double[,]> BuildAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        double[] currentValues,
        IReadOnlyList<FieldSample> primaryField
    );
}