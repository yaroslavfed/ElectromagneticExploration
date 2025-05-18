using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Enums;

namespace Direct.Core.Services.StaticServices.SensorGenerator;

public static class SensorGenerator
{
    public static List<Sensor> GenerateXYPlaneSensors(
        double xMin,
        double xMax,
        int xCount,
        double yMin,
        double yMax,
        int yCount,
        double zLevel,
        ESensorComponent component
    )
    {
        var sensors = new List<Sensor>();

        double dx = (xMax - xMin) / (xCount - 1);
        double dy = (yMax - yMin) / (yCount - 1);

        for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < yCount; j++)
            {
                sensors.Add(
                    new()
                    {
                        Position = new() { X = xMin + i * dx, Y = yMin + j * dy, Z = zLevel },
                        ComponentDirection = component
                    }
                );
            }
        }

        return sensors;
    }
}