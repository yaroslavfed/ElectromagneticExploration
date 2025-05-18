using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.SolutionExportService;

public interface ISolutionExportService
{
    void ExportToJson(IReadOnlyList<Edge> edges, Vector solution, string filePath);

    void ExportFullBFieldToJson(IReadOnlyList<Sensor> sensors, Mesh mesh, Vector solution, string filePath);

    void ExportBFieldToVtu(IReadOnlyList<Sensor> sensors, Mesh mesh, Vector solution, string filePath);

    void ExportSensorsToJson(IReadOnlyList<Sensor> sensors, Mesh mesh, Vector solution, string filePath);
}