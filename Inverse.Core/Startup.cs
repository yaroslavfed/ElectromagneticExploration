using Direct.Core.Services.DirectTaskService;
using Direct.Core.Services.SourceProvider;
using Direct.Core.Services.StaticServices.SensorGenerator;
using Direct.Core.Services.StaticServices.TestSessionParser;
using Direct.Core.Services.TestSessionService;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Enums;
using Electromagnetic.Common.Models;
using Electromagnetic.Common.Services;
using Inverse.GaussNewton.Services.GaussNewtonInversionService;

namespace Inverse.Core;

internal class Startup(
    IDirectTaskService directTaskService,
    ITestSessionService testSessionService,
    ICurrentSourceProvider currentSourceProvider,
    IGaussNewtonInversionService adaptiveInversionService
)
{
    private IReadOnlyList<FieldSample> _primaryField = [];

    public async Task Run()
    {
        // Параметры сетки инверсии
        var initialParameters = await InitializeInverseTestSession();

        // Параметры алгоритма инверсии
        var inversionOptions
            = await ModelFromJsonLoader.LoadOptionsAsync<InverseOptions>(
                "InverseInitialParameters\\inverse_options.json"
            );

        // Параметры алгоритма адаптации сетки
        var refinementOptions
            = await ModelFromJsonLoader.LoadOptionsAsync<MeshRefinementOptions>(
                "InverseInitialParameters\\mesh_refinement_options.json"
            );

        // Получение истинных параметров сетки
        var trueTestSession = await InitializeDirectTestSession();

        // Магнитная проницаемость среды
        var baseMu = initialParameters.AdditionParameters.MuCoefficient;

        // Построение сетки для инверсии
        var initialMesh = (await testSessionService.CreateTestSessionAsync(initialParameters)).Mesh;

        // Источники тока
        var trueSources = await currentSourceProvider.GetSourcesAsync(trueTestSession.CurrentSource);

        // Истинное значение на сенсорах
        var trueSensors = CreateTrueSensorsModel(trueTestSession);

        await CalculateBaseEnvironment(initialMesh, trueSensors, trueSources, baseMu);

        // Получение истинных значений на сенсорах
        Console.WriteLine("Getting true values");
        var trueModelValues = await CalculateTrueModelValuesModel(trueTestSession);

        await adaptiveInversionService.AdaptiveInvertAsync(
            trueModelValues,
            trueSources,
            trueSensors,
            _primaryField,
            baseMu,
            initialMesh,
            inversionOptions,
            refinementOptions
        );
    }

    private async Task CalculateBaseEnvironment(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        double baseMu
    )
    {
        if (_primaryField.Count == 0)
        {
            var homogeneousMesh = new Mesh
            {
                Elements = mesh
                           .Elements
                           .Select(e => new FiniteElement
                               {
                                   Mu = baseMu,
                                   Edges = e
                                           .Edges
                                           .Select(edge => new Edge
                                               {
                                                   EdgeIndex = edge.EdgeIndex,
                                                   Nodes = edge
                                                           .Nodes
                                                           .Select(n => new Node
                                                               {
                                                                   NodeIndex = n.NodeIndex,
                                                                   Coordinate = new Point3D(
                                                                       n.Coordinate.X,
                                                                       n.Coordinate.Y,
                                                                       n.Coordinate.Z
                                                                   )
                                                               }
                                                           )
                                                           .ToList()
                                               }
                                           )
                                           .ToList()
                               }
                           )
                           .ToList()
            };

            _primaryField = await directTaskService.CalculateDirectTaskAsync(
                homogeneousMesh,
                sensors,
                sources
            );
        }
    }

    private async Task<TestSession> InitializeInverseTestSession()
    {
        var json = await File.ReadAllTextAsync("InverseInitialParameters\\initial_mesh_options.json");
        var testSession = TestSessionParser.ParseFromJson(json);

        return testSession;
    }

    private async Task<IReadOnlyList<FieldSample>> CalculateTrueModelValuesModel(TestSession testSessionParameters)
    {
        if (testSessionParameters.Sensors.Count == 0)
            InitializeSensors(testSessionParameters);

        return await directTaskService.CalculateDirectTaskAsync(testSessionParameters, _primaryField);
    }

    private IReadOnlyList<Sensor> CreateTrueSensorsModel(TestSession testSessionParameters)
    {
        if (testSessionParameters.Sensors.Count == 0)
            InitializeSensors(testSessionParameters);

        return testSessionParameters.Sensors;
    }

    private async Task<TestSession> InitializeDirectTestSession()
    {
        var json = await File.ReadAllTextAsync("DirectInitialParameters\\direct_initial_parameters.json");
        var testSession = TestSessionParser.ParseFromJson(json);

        return testSession;
    }

    private void InitializeSensors(TestSession testSessionParameters)
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
    }
}