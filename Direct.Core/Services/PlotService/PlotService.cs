using System.Diagnostics;
using Electromagnetic.Common.Data.Domain;
using Newtonsoft.Json;

namespace Direct.Core.Services.PlotService;

public class PlotService : IPlotService
{
    private Task CreateDataFiles(Mesh mesh, IReadOnlyList<Sensor> sensors)
    {
        var model = new
        {
            mesh.Elements,
            sensors = sensors.Select(sensor => new
                {
                    Position = new { sensor.Position.X, sensor.Position.Y, sensor.Position.Z },
                    ComponentDirection = sensor.ComponentDirection.ToString()
                }
            )
        };

        var json = JsonConvert.SerializeObject(model, Formatting.Indented);
        File.WriteAllText("mesh_data.json", json);

        Console.WriteLine("Data is saved to mesh_data.json");

        var outputPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "mesh_data.json"
        );

        var watcher = new FileSystemWatcher
        {
            Path = Path.GetDirectoryName(outputPath) ?? throw new InvalidOperationException(),
            Filter = Path.GetFileName(outputPath),
            EnableRaisingEvents = true
        };

        watcher.Changed += (s, e) => Console.WriteLine("Data updated - ready for visualization");

        return Task.CompletedTask;
    }

    public async Task ShowPlotAsync(Mesh mesh, IReadOnlyList<Sensor> sensors)
    {
        await CreateDataFiles(mesh, sensors);

        Console.WriteLine("Start drowning mesh plot");
        using Process myProcess = new();
        myProcess.StartInfo.FileName = "python";
        myProcess.StartInfo.Arguments = @"Scripts/show_plots_script.py";
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.RedirectStandardInput = true;
        myProcess.StartInfo.RedirectStandardOutput = false;
        myProcess.Start();
        Console.WriteLine("End drowning mesh plot");
    }
}