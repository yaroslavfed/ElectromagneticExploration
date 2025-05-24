using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.AssemblyService;

public interface IAssemblyService
{
    Task<Matrix> AssembleStateStiffnessMatrixAsync(Mesh mesh);

    Task<(Matrix GlobalMatrix, Vector GlobalRhs)> AssembleGlobalSystemAsync(
        Mesh mesh,
        IReadOnlyList<CurrentSegment> sources
    );
}