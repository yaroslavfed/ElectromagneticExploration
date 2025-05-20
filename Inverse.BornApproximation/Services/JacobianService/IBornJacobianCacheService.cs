using Electromagnetic.Common.Data.Domain;

namespace Inverse.BornApproximation.Services.JacobianService;

public interface IBornJacobianCacheService
{
    Task<double[,]> BuildOnceAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        double[] currentValues,
        IReadOnlyList<FieldSample> primaryField
    );
}