using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.VisualizerService;

public interface IVisualizerService
{
    Task DrawMeshPlotAsync(Mesh mesh);
}