using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Models.BasicFunction;

public interface IBasicFunction
{
    Vector3D GetBasicFunctions(FiniteElement finiteElement, int? number, Point3D? position);

    Vector3D GetCurl(FiniteElement element, int number, Point3D point);
}