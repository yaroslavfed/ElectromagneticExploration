using AutoMapper;
using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Models.BasicFunction;

public class BasicFunction : IBasicFunction
{
    private readonly IMapper _mapper;

    public BasicFunction(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    public Vector3D GetBasicFunctions(FiniteElement finiteElement, int? number, Point3D? position)
    {
        var feBounds = MapFiniteElementsAsync(finiteElement);

        return (number + 1) switch
        {
            1 => new()
            {
                X = HierarchicalFunctionsMinus(feBounds.LowCoordinate.Y, feBounds.HighCoordinate.Y, position!.Y)
                    * HierarchicalFunctionsMinus(feBounds.LowCoordinate.Z, feBounds.HighCoordinate.Z, position.Z),
                Y = 0,
                Z = 0
            },
            2 => new()
            {
                X = HierarchicalFunctionsPlus(feBounds.LowCoordinate.Y, feBounds.HighCoordinate.Y, position!.Y)
                    * HierarchicalFunctionsMinus(feBounds.LowCoordinate.Z, feBounds.HighCoordinate.Z, position.Z),
                Y = 0,
                Z = 0
            },
            3 => new()
            {
                X = HierarchicalFunctionsMinus(feBounds.LowCoordinate.Y, feBounds.HighCoordinate.Y, position!.Y)
                    * HierarchicalFunctionsPlus(feBounds.LowCoordinate.Z, feBounds.HighCoordinate.Z, position.Z),
                Y = 0,
                Z = 0
            },
            4 => new()
            {
                X = HierarchicalFunctionsPlus(feBounds.LowCoordinate.Y, feBounds.HighCoordinate.Y, position!.Y)
                    * HierarchicalFunctionsPlus(feBounds.LowCoordinate.Z, feBounds.HighCoordinate.Z, position.Z),
                Y = 0,
                Z = 0
            },
            5 => new()
            {
                X = 0,
                Y = HierarchicalFunctionsMinus(feBounds.LowCoordinate.X, feBounds.HighCoordinate.X, position!.X)
                    * HierarchicalFunctionsMinus(feBounds.LowCoordinate.Z, feBounds.HighCoordinate.Z, position.Z),
                Z = 0
            },
            6 => new()
            {
                X = 0,
                Y = HierarchicalFunctionsPlus(feBounds.LowCoordinate.X, feBounds.HighCoordinate.X, position!.X)
                    * HierarchicalFunctionsMinus(feBounds.LowCoordinate.Z, feBounds.HighCoordinate.Z, position.Z),
                Z = 0
            },
            7 => new()
            {
                X = 0,
                Y = HierarchicalFunctionsMinus(feBounds.LowCoordinate.X, feBounds.HighCoordinate.X, position!.X)
                    * HierarchicalFunctionsPlus(feBounds.LowCoordinate.Z, feBounds.HighCoordinate.Z, position.Z),
                Z = 0
            },
            8 => new()
            {
                X = 0,
                Y = HierarchicalFunctionsPlus(feBounds.LowCoordinate.X, feBounds.HighCoordinate.X, position!.X)
                    * HierarchicalFunctionsPlus(feBounds.LowCoordinate.Z, feBounds.HighCoordinate.Z, position.Z),
                Z = 0
            },
            9 => new()
            {
                X = 0,
                Y = 0,
                Z = HierarchicalFunctionsMinus(feBounds.LowCoordinate.X, feBounds.HighCoordinate.X, position!.X)
                    * HierarchicalFunctionsMinus(feBounds.LowCoordinate.Y, feBounds.HighCoordinate.Y, position.Y)
            },
            10 => new()
            {
                X = 0,
                Y = 0,
                Z = HierarchicalFunctionsPlus(feBounds.LowCoordinate.X, feBounds.HighCoordinate.X, position!.X)
                    * HierarchicalFunctionsMinus(feBounds.LowCoordinate.Y, feBounds.HighCoordinate.Y, position.Y)
            },
            11 => new()
            {
                X = 0,
                Y = 0,
                Z = HierarchicalFunctionsMinus(feBounds.LowCoordinate.X, feBounds.HighCoordinate.X, position!.X)
                    * HierarchicalFunctionsPlus(feBounds.LowCoordinate.Y, feBounds.HighCoordinate.Y, position.Y)
            },
            12 => new()
            {
                X = 0,
                Y = 0,
                Z = HierarchicalFunctionsPlus(feBounds.LowCoordinate.X, feBounds.HighCoordinate.X, position!.X)
                    * HierarchicalFunctionsPlus(feBounds.LowCoordinate.Y, feBounds.HighCoordinate.Y, position.Y)
            },
            _ => throw new ArgumentOutOfRangeException($"{number}")
        };
    }

    /// <inheritdoc />
    public Vector3D GetCurl(FiniteElement element, int number, Point3D point)
    {
        const double h = 1e-4;

        // Малые смещения по каждой оси
        var dx = new Point3D(point.X + h, point.Y, point.Z);
        var dy = new Point3D(point.X, point.Y + h, point.Z);
        var dz = new Point3D(point.X, point.Y, point.Z + h);

        var fx = GetBasicFunctions(element, number, new() { X = point.X, Y = point.Y, Z = point.Z });
        
        var fxDx = GetBasicFunctions(element, number, new() { X = dx.X, Y = dx.Y, Z = dx.Z });
        var fxDy = GetBasicFunctions(element, number, new() { X = dy.X, Y = dy.Y, Z = dy.Z });
        var fxDz = GetBasicFunctions(element, number, new() { X = dz.X, Y = dz.Y, Z = dz.Z });

        // Производные компонент по координатам (d/dx, d/dy, d/dz)
        var dAxDy = (fxDy.X - fx.X) / h;
        var dAxDz = (fxDz.X - fx.X) / h;

        var dAyDx = (fxDx.Y - fx.Y) / h;
        var dAyDz = (fxDz.Y - fx.Y) / h;

        var dAzDx = (fxDx.Z - fx.Z) / h;
        var dAzDy = (fxDy.Z - fx.Z) / h;

        // Вычисление ротора A = (∂Az/∂y - ∂Ay/∂z, ∂Ax/∂z - ∂Az/∂x, ∂Ay/∂x - ∂Ax/∂y)
        var curlX = dAzDy - dAyDz;
        var curlY = dAxDz - dAzDx;
        var curlZ = dAyDx - dAxDy;

        return new(curlX, curlY, curlZ);
    }

    private FiniteElementBounds MapFiniteElementsAsync(FiniteElement finiteElement)
    {
        return _mapper.Map<FiniteElementBounds>(finiteElement);
    }

    private static double HierarchicalFunctionsMinus(double startPoint, double endPoint, double position) =>
        (endPoint - position) / (endPoint - startPoint);

    private static double HierarchicalFunctionsPlus(double startPoint, double endPoint, double position) =>
        (position - startPoint) / (endPoint - startPoint);
}