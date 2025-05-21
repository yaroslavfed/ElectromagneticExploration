using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.BasisFunctionProvider;

public class BasicFunctionProvider : IBasisFunctionProvider
{
    private const double H = 1e-2;

    public Vector3D GetCurl(FiniteElement element, int number, Point3D point)
    {
        var f = GetBasicFunctions(element, number, point);

        var fDx = GetBasicFunctions(element, number, new Point3D(point.X + H, point.Y, point.Z));
        var fDy = GetBasicFunctions(element, number, new Point3D(point.X, point.Y + H, point.Z));
        var fDz = GetBasicFunctions(element, number, new Point3D(point.X, point.Y, point.Z + H));

        double curlX = (fDz.Y - f.Y) / H - (fDy.Z - f.Z) / H;
        double curlY = (fDx.Z - f.Z) / H - (fDz.X - f.X) / H;
        double curlZ = (fDy.X - f.X) / H - (fDx.Y - f.Y) / H;

        return new Vector3D(curlX, curlY, curlZ);
    }

    public Vector3D GetValue(FiniteElement element, int edgeNumber, Point3D point)
    {
        return GetBasicFunctions(element, edgeNumber, point);
    }

    private Vector3D GetBasicFunctions(FiniteElement finiteElement, int number, Point3D position)
    {
        var pos = position;
        var bounds = finiteElement.GetBounds();

        double xMin = bounds.MinX, xMax = bounds.MaxX;
        double yMin = bounds.MinY, yMax = bounds.MaxY;
        double zMin = bounds.MinZ, zMax = bounds.MaxZ;

        return (number + 1) switch
        {
            1 => new(
                HierarchicalFunctionsMinus(yMin, yMax, pos.Y) * HierarchicalFunctionsMinus(zMin, zMax, pos.Z),
                0,
                0
            ),
            2 => new(
                HierarchicalFunctionsPlus(yMin, yMax, pos.Y) * HierarchicalFunctionsMinus(zMin, zMax, pos.Z),
                0,
                0
            ),
            3 => new(
                HierarchicalFunctionsMinus(yMin, yMax, pos.Y) * HierarchicalFunctionsPlus(zMin, zMax, pos.Z),
                0,
                0
            ),
            4 => new(HierarchicalFunctionsPlus(yMin, yMax, pos.Y) * HierarchicalFunctionsPlus(zMin, zMax, pos.Z), 0, 0),

            5 => new(
                0,
                HierarchicalFunctionsMinus(xMin, xMax, pos.X) * HierarchicalFunctionsMinus(zMin, zMax, pos.Z),
                0
            ),
            6 => new(
                0,
                HierarchicalFunctionsPlus(xMin, xMax, pos.X) * HierarchicalFunctionsMinus(zMin, zMax, pos.Z),
                0
            ),
            7 => new(
                0,
                HierarchicalFunctionsMinus(xMin, xMax, pos.X) * HierarchicalFunctionsPlus(zMin, zMax, pos.Z),
                0
            ),
            8 => new(0, HierarchicalFunctionsPlus(xMin, xMax, pos.X) * HierarchicalFunctionsPlus(zMin, zMax, pos.Z), 0),

            9 => new(
                0,
                0,
                HierarchicalFunctionsMinus(xMin, xMax, pos.X) * HierarchicalFunctionsMinus(yMin, yMax, pos.Y)
            ),
            10 => new(
                0,
                0,
                HierarchicalFunctionsPlus(xMin, xMax, pos.X) * HierarchicalFunctionsMinus(yMin, yMax, pos.Y)
            ),
            11 => new(
                0,
                0,
                HierarchicalFunctionsMinus(xMin, xMax, pos.X) * HierarchicalFunctionsPlus(yMin, yMax, pos.Y)
            ),
            12 => new(
                0,
                0,
                HierarchicalFunctionsPlus(xMin, xMax, pos.X) * HierarchicalFunctionsPlus(yMin, yMax, pos.Y)
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(number), $"Edge number {number} is out of range")
        };
    }

    private static double HierarchicalFunctionsMinus(double startPoint, double endPoint, double position) =>
        (endPoint - position) / (endPoint - startPoint);

    private static double HierarchicalFunctionsPlus(double startPoint, double endPoint, double position) =>
        (position - startPoint) / (endPoint - startPoint);
}