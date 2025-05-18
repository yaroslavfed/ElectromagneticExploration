using Direct.Core.Services.BasisFunctionProvider;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Enums;

namespace Direct.Core.Services.SensorEvaluator;

public class SensorEvaluator : ISensorEvaluator
{
    private readonly IBasisFunctionProvider _basisProvider;

    public SensorEvaluator(IBasisFunctionProvider basisProvider)
    {
        _basisProvider = basisProvider;
    }

    /// <inheritdoc />
    public double EvaluateBAtSensor(Sensor sensor, Mesh mesh, Vector solution)
    {
        var b = EvaluateBVectorAt(sensor.Position, mesh, solution);
        return sensor.ComponentDirection switch
        {
            ESensorComponent.Bx => b.X,
            ESensorComponent.By => b.Y,
            ESensorComponent.Bz => b.Z,
            _                   => throw new ArgumentException("Unknown sensor component")
        };
    }

    /// <inheritdoc />
    public Vector EvaluateAll(IReadOnlyList<Sensor> sensors, Mesh mesh, Vector solution)
    {
        var result = new Vector(sensors.Count);
        for (int i = 0; i < sensors.Count; i++)
        {
            result[i] = EvaluateBAtSensor(sensors[i], mesh, solution);
        }

        return result;
    }

    /// <inheritdoc />
    public Vector3D EvaluateFullBAtPoint(Point3D point, Mesh mesh, Vector solution)
    {
        return EvaluateBVectorAt(point, mesh, solution);
    }

    /// <summary>
    /// Общая реализация вычисления вектора магнитной индукции B в заданной точке.
    /// </summary>
    private Vector3D EvaluateBVectorAt(Point3D point, Mesh mesh, Vector solution)
    {
        var element = mesh.Elements.FirstOrDefault(e => e.Contains(point));
        if (element is null)
            return new(0, 0, 0);

        var b = new Vector3D();

        for (int i = 0; i < 12; i++)
        {
            var edge = element.Edges[i];           // Локальный номер ребра
            int globalEdgeIndex = edge.EdgeIndex;  // Глобальный индекс для доступа к solution
            double Ai = solution[globalEdgeIndex]; // Коэффициент A_i

            var curl = _basisProvider.GetCurl(element, i, point);
            b.X += Ai * curl.X;
            b.Y += Ai * curl.Y;
            b.Z += Ai * curl.Z;
        }

        var mu = element.Mu > 0
            ? element.Mu
            : 1.0;
        b.X /= mu;
        b.Y /= mu;
        b.Z /= mu;

        return b;
    }
}