using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.PlotService;

public interface IPlotService
{
    Task ShowPlotAsync(Mesh mesh, IReadOnlyList<Sensor> sensors);
}