using Direct.Core.Services.ProblemService;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Extensions;

namespace Direct.Core.Services.AssemblyService;

public class AssemblyService : IAssemblyService
{
    private readonly IProblemService _problemService;

    public AssemblyService(IProblemService problemService)
    {
        _problemService = problemService;
    }

    public async Task<(Matrix GlobalMatrix, Vector GlobalRhs)> AssembleGlobalSystemAsync(
        Mesh mesh,
        IReadOnlyList<CurrentSegment> sources
    )
    {
        // Собираем все глобальные уникальные рёбра
        var globalEdges = mesh
                          .Elements
                          .SelectMany(e => e.Edges)
                          .DistinctBy(e => e.EdgeIndex)
                          .OrderBy(e => e.EdgeIndex)
                          .ToList();

        int dofCount = globalEdges.Count;

        var globalMatrix = new Matrix(dofCount, dofCount);
        var globalRhs = new Vector(dofCount);

        foreach (var element in mesh.Elements)
        {
            var localMatrix = await _problemService.AssembleElementStiffnessMatrixAsync(element);
            var localVector = await _problemService.AssembleElementRightHandVectorAsync(element, sources);

            var globalIndices = element.GetGlobalEdgeIndices();

            globalMatrix.Assemble(localMatrix, globalIndices);
            globalRhs.Assemble(localVector, globalIndices);
        }

        return (globalMatrix, globalRhs);
    }
}