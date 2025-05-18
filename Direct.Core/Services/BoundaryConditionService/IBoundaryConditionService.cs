using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.BoundaryConditionService;

public interface IBoundaryConditionService
{
    Task ApplyBoundaryConditionsAsync(Matrix matrix, Vector rhs, Mesh mesh, double eps = 1e-8);
}