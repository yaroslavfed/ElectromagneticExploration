using Electromagnetic.Common.Data.Domain;

namespace Inverse.BornApproximation.Services.JacobianService;

public interface IBornJacobianService
{
    double[,] BuildAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        Vector solutionU0
    );
}