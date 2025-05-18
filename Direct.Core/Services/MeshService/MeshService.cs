using Direct.Core.Services.NumberingService.EdgesNumberingService;
using Direct.Core.Services.NumberingService.NodesNumberingService;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Data.InputModels;
using Electromagnetic.Common.Enums;
using Electromagnetic.Common.Extensions;

namespace Direct.Core.Services.MeshService;

/// <inheritdoc cref="IMeshService"/>
public class MeshService : IMeshService
{
    private readonly IEdgesNumberingService _edgesNumberingService;
    private readonly INodesNumberingService _nodesNumberingService;

    public MeshService(IEdgesNumberingService edgesNumberingService, INodesNumberingService nodesNumberingService)
    {
        _edgesNumberingService = edgesNumberingService;
        _nodesNumberingService = nodesNumberingService;
    }

    public Task<Axis> GenerateTestConfiguration(TestSession testSession)
    {
        var axis = new Axis
        {
            Positioning
                = new()
                {
                    CenterCoordinate = new()
                    {
                        X = testSession.MeshParameters.XCenterCoordinate,
                        Y = testSession.MeshParameters.YCenterCoordinate,
                        Z = testSession.MeshParameters.ZCenterCoordinate
                    },
                    BoundsDistance
                        = new()
                        {
                            X = testSession.MeshParameters.XStepToBounds,
                            Y = testSession.MeshParameters.YStepToBounds,
                            Z = testSession.MeshParameters.ZStepToBounds
                        }
                },
            Splitting = new()
            {
                SplittingCoefficient = new()
                {
                    X = testSession.SplittingParameters.XSplittingCoefficient,
                    Y = testSession.SplittingParameters.YSplittingCoefficient,
                    Z = testSession.SplittingParameters.ZSplittingCoefficient
                },
                MultiplyCoefficient
                    = new()
                    {
                        X = testSession.SplittingParameters.XMultiplyCoefficient,
                        Y = testSession.SplittingParameters.YMultiplyCoefficient,
                        Z = testSession.SplittingParameters.ZMultiplyCoefficient
                    }
            },
            AdditionalParameters = new()
            {
                Mu = testSession.AdditionParameters.MuCoefficient,
                Gamma = testSession.AdditionParameters.GammaCoefficient,
                BoundaryCondition = (EBoundaryConditions)testSession.AdditionParameters.BoundaryCondition
            },
            StrataList = testSession.StrataList,
        };

        return Task.FromResult(axis);
    }

    public async Task<Mesh> GenerateMeshAsync(Axis meshModel)
    {
        var pointsList = await ConfigureAnomalyPointsListAsync(meshModel);

        var nx = pointsList.Select(points => points.X).Distinct().ToArray().Length;
        var ny = pointsList.Select(points => points.Y).Distinct().ToArray().Length;
        var nz = pointsList.Select(points => points.Z).Distinct().ToArray().Length;

        var finiteElements = Enumerable
                             .Range(0, (nx - 1) * (ny - 1) * (nz - 1))
                             .Select(_ => new FiniteElementWithNumerics())
                             .ToArray();

        await _nodesNumberingService.ConfigureGlobalNumbering(nx, ny, nz, finiteElements);
        await _edgesNumberingService.ConfigureGlobalNumbering(nx, ny, nz, finiteElements);

        var mesh = new Mesh
        {
            Elements = finiteElements
                       .Select(element => new FiniteElement
                           {
                               Edges = element
                                       .MapNodesEdges
                                       .Select((associationPoints, edgeIndex) => new Edge
                                           {
                                               EdgeIndex = element.Edges[edgeIndex],
                                               Nodes =
                                               [
                                                   new()
                                                   {
                                                       NodeIndex = associationPoints.First,
                                                       Coordinate = new()
                                                       {
                                                           X = pointsList[associationPoints.First].X,
                                                           Y = pointsList[associationPoints.First].Y,
                                                           Z = pointsList[associationPoints.First].Z
                                                       }
                                                   },
                                                   new()
                                                   {
                                                       NodeIndex = associationPoints.Second,
                                                       Coordinate = new()
                                                       {
                                                           X = pointsList[associationPoints.Second].X,
                                                           Y = pointsList[associationPoints.Second].Y,
                                                           Z = pointsList[associationPoints.Second].Z
                                                       }
                                                   }
                                               ]
                                           }
                                       )
                                       .ToList()
                           }
                       )
                       .ToList()
        };

        return mesh;
    }

    /// <inheritdoc />
    public Task AssignMuesAsync(Mesh mesh, Axis meshModel)
    {
        foreach (var element in mesh.Elements)
        {
            var center = GetElementCenter(element);
            var matchingStrata
                = meshModel.StrataList.FirstOrDefault(strata => IsPointInsideStrata(center, strata.Positioning));

            element.Mu = matchingStrata?.Mu ?? meshModel.AdditionalParameters.Mu;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Вычисляет центр конечного элемента как среднее координат его узлов.
    /// </summary>
    private Point3D GetElementCenter(FiniteElement element)
    {
        var allNodes = element.Edges.SelectMany(e => e.Nodes).Distinct().ToList();
        double x = allNodes.Average(node => node.Coordinate.X);
        double y = allNodes.Average(node => node.Coordinate.Y);
        double z = allNodes.Average(node => node.Coordinate.Z);

        return new() { X = x, Y = y, Z = z };
    }

    /// <summary>
    /// Проверяет, находится ли точка внутри Strata.
    /// </summary>
    private bool IsPointInsideStrata(Point3D point, Positioning positioning)
    {
        return point.X >= positioning.CenterCoordinate.X - positioning.BoundsDistance.X
               && point.X <= positioning.CenterCoordinate.X + positioning.BoundsDistance.X
               && point.Y >= positioning.CenterCoordinate.Y - positioning.BoundsDistance.Y
               && point.Y <= positioning.CenterCoordinate.Y + positioning.BoundsDistance.Y
               && point.Z >= positioning.CenterCoordinate.Z - positioning.BoundsDistance.Z
               && point.Z <= positioning.CenterCoordinate.Z + positioning.BoundsDistance.Z;
    }

    /// <summary>
    ///  Получение списка точек из параметров конфигурации расчётной области с учётом физических объектов
    /// </summary>
    /// <param name="meshParameters">Входные параметры модели сетки</param>
    /// <returns>Список точек принадлежащих расчётной области</returns>
    private async Task<List<Point3D>> ConfigureAnomalyPointsListAsync(Axis meshParameters)
    {
        var mainAreaPoints = await ConfigurePointsListAsync(meshParameters);
        var mainAreaX = mainAreaPoints.Select(point3D => point3D.X).ToArray();
        var mainAreaY = mainAreaPoints.Select(point3D => point3D.Y).ToArray();
        var mainAreaZ = mainAreaPoints.Select(point3D => point3D.Z).ToArray();

        // var mainAreaPoints = meshParameters.Positioning.GetPoints();
        // var mainAreaX = mainAreaPoints["x"].ToList();
        // var mainAreaY = mainAreaPoints["y"].ToList();
        // var mainAreaZ = mainAreaPoints["z"].ToList();

        var minX = mainAreaX.Min();
        var maxX = mainAreaX.Max();
        var minY = mainAreaY.Min();
        var maxY = mainAreaY.Max();
        var minZ = mainAreaZ.Min();
        var maxZ = mainAreaZ.Max();

        var anomalies = meshParameters.StrataList.Select(strata => strata.Positioning.GetPoints()).ToList();

        var filteredAnomaliesX = anomalies
                                 .SelectMany(p => p["x"])
                                 .Where(x => x > minX && x < maxX)
                                 .Concat(mainAreaX)
                                 .Order()
                                 .Distinct()
                                 .ToList();
        var filteredAnomaliesY = anomalies
                                 .SelectMany(p => p["y"])
                                 .Where(y => y > minY && y < maxY)
                                 .Concat(mainAreaY)
                                 .Order()
                                 .Distinct()
                                 .ToList();
        var filteredAnomaliesZ = anomalies
                                 .SelectMany(p => p["z"])
                                 .Where(z => z > minZ && z < maxZ)
                                 .Concat(mainAreaZ)
                                 .Order()
                                 .Distinct()
                                 .ToList();

        var strataMesh = (from itemZ in filteredAnomaliesZ
                          from itemY in filteredAnomaliesY
                          from itemX in filteredAnomaliesX
                          select new Point3D { X = itemX, Y = itemY, Z = itemZ }).ToList();

        return strataMesh;
    }

    /// <summary>
    /// Получение списка точек из параметров конфигурации расчётной области
    /// </summary>
    /// <param name="meshParameters">Входные параметры сетки</param>
    /// <returns>Список точек принадлежащих расчётной области</returns>
    private static Task<List<Point3D>> ConfigurePointsListAsync(Axis meshParameters)
    {
        var x = MathExtensions.SplitAxis(
            meshParameters.Splitting.MultiplyCoefficient.X,
            (int)meshParameters.Splitting.SplittingCoefficient.X,
            meshParameters.Positioning.GetHighPoint3D().X,
            meshParameters.Positioning.GetLowPoint3D().X
        );

        var y = MathExtensions.SplitAxis(
            meshParameters.Splitting.MultiplyCoefficient.Y,
            (int)meshParameters.Splitting.SplittingCoefficient.Y,
            meshParameters.Positioning.GetHighPoint3D().Y,
            meshParameters.Positioning.GetLowPoint3D().Y
        );

        var z = MathExtensions.SplitAxis(
            meshParameters.Splitting.MultiplyCoefficient.Z,
            (int)meshParameters.Splitting.SplittingCoefficient.Z,
            meshParameters.Positioning.GetHighPoint3D().Z,
            meshParameters.Positioning.GetLowPoint3D().Z
        );

        var strataMesh
            = (from itemZ in z from itemY in y from itemX in x select new Point3D { X = itemX, Y = itemY, Z = itemZ })
            .ToList();

        return Task.FromResult(strataMesh);
    }
}