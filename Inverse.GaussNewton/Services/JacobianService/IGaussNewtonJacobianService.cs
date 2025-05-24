using Electromagnetic.Common.Data.Domain;

namespace Inverse.GaussNewton.Services.JacobianService;

public interface IGaussNewtonJacobianService
{
    Task<double[,]> BuildAsync(
        int iteration,
        int maxIterations,
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<FieldSample> currentModelValues,
        IReadOnlyList<FieldSample> primaryField
    );
}