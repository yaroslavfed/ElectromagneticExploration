using Electromagnetic.Common.Data.Domain;

namespace Inverse.SharedCore.DirectTaskService;

public interface IDirectTaskService
{
    Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<FieldSample> primaryField
    );

    Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        TestSession testSessionParameters,
        IReadOnlyList<FieldSample> primaryField
    );

    Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources
    );

    Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(TestSession testSessionParameters);
}