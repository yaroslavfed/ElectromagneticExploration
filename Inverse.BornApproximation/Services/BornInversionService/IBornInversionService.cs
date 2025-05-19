using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;

namespace Inverse.BornApproximation.Services.BornInversionService;

public interface IBornInversionService
{
    Task<double[]> InvertIterativelyAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        double[] observedValues,
        double baseMu,
        InverseOptions options,
        int maxIterations,
        double functionalThreshold = 1e-8
    );
}