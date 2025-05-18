using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.BasisFunctionProvider;

public interface IBasisFunctionProvider
{
    Vector3D GetValue(FiniteElement element, int edgeNumber, Point3D point);

    Vector3D GetCurl(FiniteElement element, int edgeNumber, Point3D point);
}