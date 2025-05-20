using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;

namespace Inverse.BornApproximation.Services.BornInversionService;

public interface IBornInversionService
{
    Task AdaptiveInvertAsync(
        IReadOnlyList<FieldSample> trueModelValues,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<FieldSample> primaryField,
        double baseMu,
        Mesh initialMesh,
        InverseOptions inversionOptions,
        MeshRefinementOptions refinementOptions
    );
}