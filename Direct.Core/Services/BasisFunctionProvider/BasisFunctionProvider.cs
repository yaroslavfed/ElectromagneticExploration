using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.BasisFunctionProvider;

public class BasicFunctionProvider : IBasisFunctionProvider
{
    public Vector3D GetCurl(FiniteElement element, int number, Point3D point)
    {
        const double h = 1e-2;
        
        var f = GetBasicFunctions(element, number, point);

        var dx = new Point3D(point.X + h, point.Y, point.Z);
        var dy = new Point3D(point.X, point.Y + h, point.Z);
        var dz = new Point3D(point.X, point.Y, point.Z + h);

        var f_dx = GetBasicFunctions(element, number, dx);
        var f_dy = GetBasicFunctions(element, number, dy);
        var f_dz = GetBasicFunctions(element, number, dz);

        double curlX = (f_dz.Y - f.Y) / h - (f_dy.Z - f.Z) / h;
        double curlY = (f_dx.Z - f.Z) / h - (f_dz.X - f.X) / h;
        double curlZ = (f_dy.X - f.X) / h - (f_dx.Y - f.Y) / h;

        return new(curlX, curlY, curlZ);
    }

    public Vector3D GetValue(FiniteElement element, int edgeNumber, Point3D point)
    {
        return GetBasicFunctions(element, edgeNumber, point);
    }

    private Vector3D GetBasicFunctions(FiniteElement finiteElement, int? number, Point3D? position)
    {
        return (number + 1) switch
        {
            1 => new()
            {
                X = HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinY,
                        finiteElement.GetBounds().MaxY,
                        position!.Y
                    )
                    * HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinZ,
                        finiteElement.GetBounds().MaxZ,
                        position.Z
                    ),
                Y = 0,
                Z = 0
            },
            2 => new()
            {
                X = HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinY,
                        finiteElement.GetBounds().MaxY,
                        position!.Y
                    )
                    * HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinZ,
                        finiteElement.GetBounds().MaxZ,
                        position.Z
                    ),
                Y = 0,
                Z = 0
            },
            3 => new()
            {
                X = HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinY,
                        finiteElement.GetBounds().MaxY,
                        position!.Y
                    )
                    * HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinZ,
                        finiteElement.GetBounds().MaxZ,
                        position.Z
                    ),
                Y = 0,
                Z = 0
            },
            4 => new()
            {
                X = HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinY,
                        finiteElement.GetBounds().MaxY,
                        position!.Y
                    )
                    * HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinZ,
                        finiteElement.GetBounds().MaxZ,
                        position.Z
                    ),
                Y = 0,
                Z = 0
            },
            5 => new()
            {
                X = 0,
                Y = HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinX,
                        finiteElement.GetBounds().MaxX,
                        position!.X
                    )
                    * HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinZ,
                        finiteElement.GetBounds().MaxZ,
                        position.Z
                    ),
                Z = 0
            },
            6 => new()
            {
                X = 0,
                Y = HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinX,
                        finiteElement.GetBounds().MaxX,
                        position!.X
                    )
                    * HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinZ,
                        finiteElement.GetBounds().MaxZ,
                        position.Z
                    ),
                Z = 0
            },
            7 => new()
            {
                X = 0,
                Y = HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinX,
                        finiteElement.GetBounds().MaxX,
                        position!.X
                    )
                    * HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinZ,
                        finiteElement.GetBounds().MaxZ,
                        position.Z
                    ),
                Z = 0
            },
            8 => new()
            {
                X = 0,
                Y = HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinX,
                        finiteElement.GetBounds().MaxX,
                        position!.X
                    )
                    * HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinZ,
                        finiteElement.GetBounds().MaxZ,
                        position.Z
                    ),
                Z = 0
            },
            9 => new()
            {
                X = 0,
                Y = 0,
                Z = HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinX,
                        finiteElement.GetBounds().MaxX,
                        position!.X
                    )
                    * HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinY,
                        finiteElement.GetBounds().MaxY,
                        position.Y
                    )
            },
            10 => new()
            {
                X = 0,
                Y = 0,
                Z = HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinX,
                        finiteElement.GetBounds().MaxX,
                        position!.X
                    )
                    * HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinY,
                        finiteElement.GetBounds().MaxY,
                        position.Y
                    )
            },
            11 => new()
            {
                X = 0,
                Y = 0,
                Z = HierarchicalFunctionsMinus(
                        finiteElement.GetBounds().MinX,
                        finiteElement.GetBounds().MaxX,
                        position!.X
                    )
                    * HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinY,
                        finiteElement.GetBounds().MaxY,
                        position.Y
                    )
            },
            12 => new()
            {
                X = 0,
                Y = 0,
                Z = HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinX,
                        finiteElement.GetBounds().MaxX,
                        position!.X
                    )
                    * HierarchicalFunctionsPlus(
                        finiteElement.GetBounds().MinY,
                        finiteElement.GetBounds().MaxY,
                        position.Y
                    )
            },
            _ => throw new ArgumentOutOfRangeException($"{number}")
        };
    }

    private static double HierarchicalFunctionsMinus(double startPoint, double endPoint, double position) =>
        (endPoint - position) / (endPoint - startPoint);

    private static double HierarchicalFunctionsPlus(double startPoint, double endPoint, double position) =>
        (position - startPoint) / (endPoint - startPoint);
}