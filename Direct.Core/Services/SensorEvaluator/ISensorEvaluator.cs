using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.SensorEvaluator;

public interface ISensorEvaluator
{
    /// <summary>
    /// Вычисляет значение одной компоненты магнитного поля B в заданной точке (сенсоре).
    /// </summary>
    double EvaluateBAtSensor(Sensor sensor, Mesh mesh, Vector solution);

    /// <summary>
    /// Вычисляет значения поля B во всех сенсорах.
    /// </summary>
    Vector EvaluateAll(IReadOnlyList<Sensor> sensors, Mesh mesh, Vector solution);

    Vector3D EvaluateFullBAtPoint(Point3D point, Mesh mesh, Vector solution);
}