using Direct.Core.Services.DirectTaskService;
using Direct.Core.Services.SourceProvider;
using Direct.Core.Services.StaticServices.SensorGenerator;
using Direct.Core.Services.StaticServices.TestSessionParser;
using Direct.Core.Services.TestSessionService;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Enums;
using Electromagnetic.Common.Models;
using Electromagnetic.Common.Services;
using Inverse.BornApproximation.Services.BornInversionService;
using Inverse.GaussNewton.Services.GaussNewtonInversionService;
using Inverse.SharedDTO;

namespace Inverse.Core;

internal class Startup(
    IDirectTaskService directTaskService,
    ITestSessionService testSessionService,
    ICurrentSourceProvider currentSourceProvider,
    IGaussNewtonInversionService gaussNewtonInversionService,
    IBornInversionService bornInversionService
)
{
    private IReadOnlyList<FieldSample> _primaryField = [];

    public async Task Run()
    {
        var inversionType = GetInversionType();

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

        // Истинное положение сенсоров
        var trueSensors = CreateTrueSensorsModel(trueTestSession);

        Console.WriteLine("Calculate base environment");
        await CalculateBaseEnvironment(initialMesh, trueSensors, trueSources, baseMu);

        // Получение истинных значений на сенсорах
        Console.WriteLine("Getting true values");
        var trueModelValues = await CalculateTrueModelValuesModel(trueTestSession);

        switch (inversionType)
        {
            case EInversionType.GaussNewtonMethod:
                await RunGaussNewtonMethodInversionAsync(
                    trueModelValues,
                    trueSources,
                    trueSensors,
                    _primaryField,
                    baseMu,
                    initialMesh,
                    inversionOptions,
                    refinementOptions
                );
                break;
            case EInversionType.BornApproximation:
                await RunBornApproximationInversionAsync(
                    trueModelValues,
                    trueSources,
                    trueSensors,
                    _primaryField,
                    baseMu,
                    initialMesh,
                    inversionOptions,
                    refinementOptions
                );
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private EInversionType GetInversionType()
    {
        Console.WriteLine("1 - GaussNewton\t2 - BornApproximation");
        var command = int.TryParse(Console.ReadLine(), out var number);

        return command switch
        {
            true when number == 1 => EInversionType.GaussNewtonMethod,
            true when number == 2 => EInversionType.BornApproximation,
            _                     => throw new ArgumentException()
        };
    }

    private async Task RunGaussNewtonMethodInversionAsync(
        IReadOnlyList<FieldSample> trueModelValues,
        IReadOnlyList<CurrentSegment> trueSources,
        IReadOnlyList<Sensor> trueSensors,
        IReadOnlyList<FieldSample> primaryField,
        double baseMu,
        Mesh initialMesh,
        InverseOptions inversionOptions,
        MeshRefinementOptions refinementOptions
    )
    {
        Console.WriteLine("GaussNewton inversion started");
        await gaussNewtonInversionService.AdaptiveInvertAsync(
            trueModelValues,
            trueSources,
            trueSensors,
            primaryField,
            baseMu,
            initialMesh,
            inversionOptions,
            refinementOptions
        );
        Console.WriteLine("GaussNewton inversion finished");
    }

    private async Task RunBornApproximationInversionAsync(
        IReadOnlyList<FieldSample> trueModelValues,
        IReadOnlyList<CurrentSegment> trueSources,
        IReadOnlyList<Sensor> trueSensors,
        IReadOnlyList<FieldSample> primaryField,
        double baseMu,
        Mesh initialMesh,
        InverseOptions inversionOptions,
        MeshRefinementOptions refinementOptions
    )
    {
        Console.WriteLine("BornApproximation inversion started");
        await bornInversionService.AdaptiveInvertAsync(
            trueModelValues,
            trueSources,
            trueSensors,
            primaryField,
            baseMu,
            initialMesh,
            inversionOptions,
            refinementOptions
        );
        Console.WriteLine("BornApproximation inversion finished");
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

            _primaryField = await directTaskService.CalculateDirectTaskAsync(homogeneousMesh, sensors, sources);
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