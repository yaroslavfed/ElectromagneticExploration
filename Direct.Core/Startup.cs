using System.Diagnostics;
using System.Text.Json;
using Direct.Core.Services.DirectTaskService;
using Direct.Core.Services.StaticServices.SensorGenerator;
using Direct.Core.Services.StaticServices.TestSessionParser;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Enums;

// ReSharper disable InconsistentNaming

namespace Direct.Core;

internal class Startup(
    IDirectTaskService directTaskService
)
{
    public async Task Run()
    {
        IReadOnlyList<FieldSample> values = [];

        Console.WriteLine("1 - both fields\t2 - only second field");
        var command = int.TryParse(Console.ReadLine(), out var number);

        values = command switch
        {
            true when number == 1 => await CalculateBothFields(),
            true when number == 2 => await CalculateOnlySecondField(),
            _                     => throw new ArgumentException()
        };

        var json = JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("field_data.json", json);
        Console.WriteLine("The file field_data.json has been created.");

        Console.WriteLine("Start drowning plot");
        var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts\\contour_plot.py");
        using Process myProcess = new();
        myProcess.StartInfo.FileName = "python";
        myProcess.StartInfo.Arguments = scriptPath;
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.RedirectStandardInput = true;
        myProcess.StartInfo.RedirectStandardOutput = false;
        myProcess.Start();
        Console.WriteLine("End drowning plot");

        Console.ReadLine();
    }

    private async Task<IReadOnlyList<FieldSample>> CalculateOnlySecondField()
    {
        var initialParameters = await InitializeTestSession();

        var emptyParameters = await InitializeTestSession();

        if (initialParameters.Sensors.Count == 0)
            await InitializeSensors(initialParameters);

        if (emptyParameters.Sensors.Count == 0)
            await InitializeSensors(emptyParameters);

        var emptyStratums = emptyParameters
                            .StrataList
                            .Select(strata => strata with
                                {
                                    Mu = strata.Mu = emptyParameters.AdditionParameters.MuCoefficient
                                }
                            )
                            .ToList();

        var emptyTestSession = emptyParameters with { StrataList = emptyStratums };

        var emptyValues = await directTaskService.CalculateDirectTaskAsync(emptyTestSession);
        var values = await directTaskService.CalculateDirectTaskAsync(initialParameters, emptyValues);

        return values;
    }

    private async Task<IReadOnlyList<FieldSample>> CalculateBothFields()
    {
        var initialParameters = await InitializeTestSession();

        if (initialParameters.Sensors.Count == 0)
            await InitializeSensors(initialParameters);

        var values = await directTaskService.CalculateDirectTaskAsync(initialParameters);

        return values;
    }

    private async Task<TestSession> InitializeTestSession()
    {
        var json = await File.ReadAllTextAsync("DirectInitialParameters\\direct_initial_parameters.json");
        var testSession = TestSessionParser.ParseFromJson(json);

        return testSession;
    }

    private Task InitializeSensors(TestSession testSessionParameters)
    {
        testSessionParameters.Sensors = SensorGenerator.GenerateXYPlaneSensors(
            xMin: -10,
            xMax: 10,
            xCount: 5,
            yMin: -10,
            yMax: 10,
            yCount: 5,
            zLevel: 0,
            component: ESensorComponent.Bz
        );

        return Task.CompletedTask;
    }
}