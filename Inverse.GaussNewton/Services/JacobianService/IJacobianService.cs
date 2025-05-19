using Electromagnetic.Common.Data.Domain;

namespace Inverse.GaussNewton.Services.JacobianService;

public interface IJacobianService
{
    Task<double[,]> BuildJacobianAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        double[] currentValues, // Значения |B| на текущей итерации
        IReadOnlyList<FieldSample> primaryField
    );
}