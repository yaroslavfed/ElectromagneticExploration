using Direct.Core.Services.AssemblyService;
using Direct.Core.Services.BasisFunctionProvider;
using Direct.Core.Services.BoundaryConditionService;
using Direct.Core.Services.PlotService;
using Direct.Core.Services.SourceProvider;
using Direct.Core.Services.TestSessionService;
using Electromagnetic.Common.Data.Domain;
using MathNet.Numerics.LinearAlgebra.Double;
using Vector = Electromagnetic.Common.Data.Domain.Vector;

// ReSharper disable InconsistentNaming

namespace Direct.Core.Services.DirectTaskService;

public class DirectTaskService(
    ITestSessionService testSessionService,
    ICurrentSourceProvider currentSourceProvider,
    IAssemblyService assemblyService,
    IBoundaryConditionService boundaryConditionService,
    IPlotService plotService,
    IBasisFunctionProvider basisFunctionProvider
) : IDirectTaskService
{
    public async Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources
    )
    {
        try
        {
            var solution = await CalculateElectroMagneticFEM(mesh, sources);

            var samples = new List<FieldSample>();
            foreach (var sensor in sensors)
            {
                var element = FindElementContaining(sensor.Position, mesh);
                var B = ComputeMagneticFieldAt(sensor.Position, element, solution);

                samples.Add(
                    new()
                    {
                        X = sensor.Position.X,
                        Y = sensor.Position.Y,
                        Z = sensor.Position.Z,
                        Bx = B.X,
                        By = B.Y,
                        Bz = B.Z,
                        Magnitude = B.Norm()
                    }
                );
            }

            return samples;
        } catch (Exception exception)
        {
            throw new(exception.ToString());
        }
    }

    public async Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<FieldSample> primaryField
    )
    {
        try
        {
            var solution = await CalculateElectroMagneticFEM(mesh, sources);

            // Вычисляем B и вычитаем B^prim
            var samples = new List<FieldSample>();

            for (int i = 0; i < sensors.Count; i++)
            {
                var sensor = sensors[i];
                var element = FindElementContaining(sensor.Position, mesh);
                var B = ComputeMagneticFieldAt(sensor.Position, element, solution);

                var Bprim = new Vector3D { X = primaryField[i].Bx, Y = primaryField[i].By, Z = primaryField[i].Bz };

                var Bsec = B - Bprim;

                samples.Add(
                    new()
                    {
                        X = sensor.Position.X,
                        Y = sensor.Position.Y,
                        Z = sensor.Position.Z,
                        Bx = Bsec.X,
                        By = Bsec.Y,
                        Bz = Bsec.Z,
                        Magnitude = Bsec.Norm()
                    }
                );
            }

            return samples;
        } catch (Exception exception)
        {
            throw new(exception.ToString());
        }
    }

    public async Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(TestSession testSessionParameters)
    {
        // Построение сетки сенсоров
        var sensors = testSessionParameters.Sensors;

        // Построение сетки
        var testSession = await testSessionService.CreateTestSessionAsync(testSessionParameters);
        await plotService.ShowPlotAsync(testSession.Mesh, sensors);

        // Источник тока
        var sources = await currentSourceProvider.GetSourcesAsync(testSessionParameters.CurrentSource);

        return await CalculateDirectTaskAsync(testSession.Mesh, sensors, sources);
    }

    public async Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        TestSession testSessionParameters,
        IReadOnlyList<FieldSample> primaryField
    )
    {
        // Построение сетки сенсоров
        var sensors = testSessionParameters.Sensors;

        // Построение сетки
        var testSession = await testSessionService.CreateTestSessionAsync(testSessionParameters);
        await plotService.ShowPlotAsync(testSession.Mesh, sensors);

        // Источник тока
        var sources = await currentSourceProvider.GetSourcesAsync(testSessionParameters.CurrentSource);

        return await CalculateDirectTaskAsync(testSession.Mesh, sensors, sources, primaryField);
    }

    private async Task<Vector> CalculateElectroMagneticFEM(Mesh mesh, IReadOnlyList<CurrentSegment> sources)
    {
        // Построение матрицы жесткости и вектора правой части
        var (globalMatrix, globalRhs) = await assemblyService.AssembleGlobalSystemAsync(mesh, sources);

        // Применение краевых условий
        await boundaryConditionService.ApplyBoundaryConditionsAsync(globalMatrix, globalRhs, mesh);

        // Построение СЛАУ
        var A = globalMatrix.ToMathNet();
        var b = globalRhs.ToMathNet();

        // Решение СЛАУ
        var At = A.Transpose();
        var lambda = 1e-2;
        var AtA = At * A;
        var I = DenseMatrix.CreateIdentity(A.ColumnCount);

        var regularized = AtA + lambda * I;
        var rhs = At * b;

        var x = regularized.Solve(rhs);

        var solution = Vector.FromMathNet(x);

        return solution;
    }

    private FiniteElement FindElementContaining(Point3D point, Mesh mesh)
    {
        foreach (var element in mesh.Elements)
        {
            var nodes = element.Edges.SelectMany(e => e.Nodes).DistinctBy(n => n.NodeIndex).ToList();

            var minX = nodes.Min(n => n.Coordinate.X);
            var maxX = nodes.Max(n => n.Coordinate.X);
            var minY = nodes.Min(n => n.Coordinate.Y);
            var maxY = nodes.Max(n => n.Coordinate.Y);
            var minZ = nodes.Min(n => n.Coordinate.Z);
            var maxZ = nodes.Max(n => n.Coordinate.Z);

            if (point.X >= minX
                && point.X <= maxX
                && point.Y >= minY
                && point.Y <= maxY
                && point.Z >= minZ
                && point.Z <= maxZ)
            {
                return element;
            }
        }

        throw new("Sensor is not inside any element.");
    }

    private Vector3D ComputeMagneticFieldAt(Point3D sensor, FiniteElement element, Vector solution)
    {
        Vector3D B = Vector3D.Zero;

        for (int i = 0; i < element.Edges.Count; i++)
        {
            var curlWi = basisFunctionProvider.GetCurl(element, i, sensor);
            var dofIndex = element.Edges[i].EdgeIndex;
            var coeff = solution[dofIndex];

            B += curlWi * coeff;
        }

        return B;
    }
}