using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Direct.Core.Services.SensorEvaluator;
using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.SolutionExportService;

public class SolutionExportService : ISolutionExportService
{
    private readonly ISensorEvaluator _sensorEvaluator;

    public SolutionExportService(ISensorEvaluator sensorEvaluator)
    {
        _sensorEvaluator = sensorEvaluator;
    }

    public void ExportToJson(IReadOnlyList<Edge> edges, Vector solution, string filePath)
    {
        var data = edges.Select((edge, i) =>
            {
                var node1 = edge.Nodes[0].Coordinate;
                var node2 = edge.Nodes[1].Coordinate;

                var center = new Point3D
                {
                    X = (node1.X + node2.X) / 2.0, Y = (node1.Y + node2.Y) / 2.0, Z = (node1.Z + node2.Z) / 2.0
                };

                var direction = new Vector3D { X = node2.X - node1.X, Y = node2.Y - node1.Y, Z = node2.Z - node1.Z }
                    .Normalize();

                return new
                {
                    x = center.X,
                    y = center.Y,
                    z = center.Z,
                    dx = direction.X,
                    dy = direction.Y,
                    dz = direction.Z,
                    value = solution[i]
                };
            }
        );

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        var json = JsonSerializer.Serialize(data, jsonOptions);
        File.WriteAllText(filePath, json);
    }

    public void ExportFullBFieldToJson(IReadOnlyList<Sensor> sensors, Mesh mesh, Vector solution, string filePath)
    {
        var data = sensors
                   .Select(sensor =>
                       {
                           var b = _sensorEvaluator.EvaluateFullBAtPoint(sensor.Position, mesh, solution);
                           return new
                           {
                               x = sensor.Position.X,
                               y = sensor.Position.Y,
                               z = sensor.Position.Z,
                               bx = b.X,
                               by = b.Y,
                               bz = b.Z
                           };
                       }
                   )
                   .ToList();

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public void ExportBFieldToVtu(IReadOnlyList<Sensor> sensors, Mesh mesh, Vector solution, string filePath)
    {
        var sb = new StringBuilder();
        var format = CultureInfo.InvariantCulture;

        sb.AppendLine(@"<?xml version=""1.0""?>");
        sb.AppendLine(@"<VTKFile type=""UnstructuredGrid"" version=""0.1"" byte_order=""LittleEndian"">");
        sb.AppendLine(@"  <UnstructuredGrid>");
        sb.AppendLine($"    <Piece NumberOfPoints=\"{sensors.Count}\" NumberOfCells=\"0\">");

        // 1. Точки
        sb.AppendLine(@"      <Points>");
        sb.AppendLine(@"        <DataArray type=""Float32"" NumberOfComponents=""3"" format=""ascii"">");
        foreach (var sensor in sensors)
        {
            sb.AppendLine(
                $"          {sensor.Position.X.ToString(format)} {sensor.Position.Y.ToString(format)} {sensor.Position.Z.ToString(format)}"
            );
        }

        sb.AppendLine(@"        </DataArray>");
        sb.AppendLine(@"      </Points>");

        // 2. Пустые ячейки
        sb.AppendLine(@"      <Cells>");
        sb.AppendLine(@"        <DataArray type=""Int32"" Name=""connectivity"" format=""ascii""/>");
        sb.AppendLine(@"        <DataArray type=""Int32"" Name=""offsets"" format=""ascii""/>");
        sb.AppendLine(@"        <DataArray type=""UInt8"" Name=""types"" format=""ascii""/>");
        sb.AppendLine(@"      </Cells>");

        // 3. Поле B
        sb.AppendLine(@"      <PointData Vectors=""B"">");
        sb.AppendLine(@"        <DataArray type=""Float32"" Name=""B"" NumberOfComponents=""3"" format=""ascii"">");

        int count = 0;
        foreach (var sensor in sensors)
        {
            try
            {
                var b = _sensorEvaluator.EvaluateFullBAtPoint(sensor.Position, mesh, solution);

                // Проверка на NaN и null
                if (b == null || double.IsNaN(b.X) || double.IsNaN(b.Y) || double.IsNaN(b.Z))
                    sb.AppendLine("          0 0 0");
                else
                    sb.AppendLine($"          {b.X.ToString(format)} {b.Y.ToString(format)} {b.Z.ToString(format)}");
            } catch
            {
                sb.AppendLine("          0 0 0"); // fallback при ошибке
            }

            count++;
        }

        sb.AppendLine(@"        </DataArray>");
        sb.AppendLine(@"      </PointData>");

        sb.AppendLine(@"    </Piece>");
        sb.AppendLine(@"  </UnstructuredGrid>");
        sb.AppendLine(@"</VTKFile>");

        File.WriteAllText(filePath, sb.ToString());

        Console.WriteLine($"▶ Экспортировано {count} точек с вектором B → {filePath}");
    }

    public void ExportSensorsToJson(IReadOnlyList<Sensor> sensors, Mesh mesh, Vector solution, string filePath)
    {
        var sensorData = sensors.Select(sensor =>
            {
                var b = _sensorEvaluator.EvaluateFullBAtPoint(sensor.Position, mesh, solution);
                return new
                {
                    x = sensor.Position.X,
                    y = sensor.Position.Y,
                    z = sensor.Position.Z,
                    bx = b?.X ?? 0.0,
                    by = b?.Y ?? 0.0,
                    bz = b?.Z ?? 0.0
                };
            }
        );

        var options = new JsonSerializerOptions
        {
            WriteIndented = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };

        File.WriteAllText(filePath, JsonSerializer.Serialize(sensorData, options));
        Console.WriteLine($"Данные сенсоров экспортированы в файл: {filePath}");
    }
}