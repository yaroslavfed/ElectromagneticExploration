using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Data.InputModels;
using Electromagnetic.Common.Enums;

namespace Electromagnetic.Common.Extensions;

public static class MathExtensions
{
    public static TResult GetBoundsPoint<TSource, TResult>(
        this IEnumerable<TSource> elements,
        Func<TSource, TResult> selector,
        EPositions position
    )
    {
        var coordinates = elements.Select(selector).ToList();
        coordinates.Sort();

        return position switch
        {
            EPositions.First => coordinates.First(),
            EPositions.Last  => coordinates.Last(),
            _                => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };
    }

    public static List<double> SplitAxis(
        double multiplyCoefficient,
        int splittingCoefficient,
        double strataLastPoint,
        double strataFirstPoint
    )
    {
        if (multiplyCoefficient == 0)
            return [strataFirstPoint, strataLastPoint];

        // Обработка по X
        var h = Math.Abs(multiplyCoefficient - 1.0) > 0E-16
            ? (strataLastPoint - strataFirstPoint)
              * (1.0 - multiplyCoefficient)
              / (1.0 - Math.Pow(multiplyCoefficient, splittingCoefficient))
            : (strataLastPoint - strataFirstPoint) / splittingCoefficient;

        List<double> axis = [strataFirstPoint];

        for (var i = 0; i < splittingCoefficient; i++)
        {
            axis.Add(axis.Last() + h);
            h *= multiplyCoefficient;
        }

        return axis;
    }

    public static Point3D GetLowPoint3D(this Positioning positioning) =>
        new()
        {
            X = positioning.CenterCoordinate.X - positioning.BoundsDistance.X,
            Y = positioning.CenterCoordinate.Y - positioning.BoundsDistance.Y,
            Z = positioning.CenterCoordinate.Z - positioning.BoundsDistance.Z
        };

    public static Point3D GetHighPoint3D(this Positioning positioning) =>
        new()
        {
            X = positioning.CenterCoordinate.X + positioning.BoundsDistance.X,
            Y = positioning.CenterCoordinate.Y + positioning.BoundsDistance.Y,
            Z = positioning.CenterCoordinate.Z + positioning.BoundsDistance.Z
        };

    public static Dictionary<string, List<double>> GetPoints(this Positioning positioning)
    {
        var xPoints = new List<double>
        {
            positioning.CenterCoordinate.X + positioning.BoundsDistance.X,
            positioning.CenterCoordinate.X - positioning.BoundsDistance.X
        };

        var yPoints = new List<double>
        {
            positioning.CenterCoordinate.Y + positioning.BoundsDistance.Y,
            positioning.CenterCoordinate.Y - positioning.BoundsDistance.Y
        };

        var zPoints = new List<double>
        {
            positioning.CenterCoordinate.Z + positioning.BoundsDistance.Z,
            positioning.CenterCoordinate.Z - positioning.BoundsDistance.Z
        };

        return new Dictionary<string, List<double>>() { { "x", xPoints }, { "y", yPoints }, { "z", zPoints } };
    }
}