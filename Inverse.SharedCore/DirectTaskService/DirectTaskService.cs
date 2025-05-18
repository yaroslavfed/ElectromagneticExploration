using Electromagnetic.Common.Data.Domain;

namespace Inverse.SharedCore.DirectTaskService;

public class DirectTaskService(
    Direct.Core.Services.DirectTaskService.IDirectTaskService directTaskService
) : IDirectTaskService
{
    public async Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<FieldSample> primaryField
    )
    {
        return await directTaskService.CalculateDirectTaskAsync(mesh, sensors, sources, primaryField);
    }

    public async Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        TestSession testSessionParameters,
        IReadOnlyList<FieldSample> primaryField
    )
    {
        return await directTaskService.CalculateDirectTaskAsync(testSessionParameters, primaryField);
    }
}