using Electromagnetic.Common.Data.Domain;

namespace Inverse.GaussNewton.Services.JacobianService;

public interface IJacobianService
{
    Task<double[,]> BuildJacobianAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<FieldSample> primaryField
    );
}