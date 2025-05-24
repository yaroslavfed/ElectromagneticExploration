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
        // Кэшируем уникальные глобальные рёбра
        var globalEdges = mesh
                          .Elements
                          .SelectMany(e => e.Edges)
                          .GroupBy(e => e.EdgeIndex)
                          .Select(g => g.First())
                          .OrderBy(e => e.EdgeIndex)
                          .ToArray();

        var dofCount = globalEdges.Length;

        var globalMatrix = new Matrix(dofCount, dofCount);
        var globalRhs = new Vector(dofCount);

        // Параллелим сборку локальных матриц и векторов
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        var locks = new object[dofCount];
        for (var i = 0; i < dofCount; i++)
            locks[i] = new();

        await Task.Run(() => Parallel.ForEach(
                           mesh.Elements,
                           parallelOptions,
                           element =>
                           {
                               var localMatrix = _problemService.AssembleElementStiffnessMatrixAsync(element).Result;
                               var localVector = _problemService.AssembleElementRightHandVectorAsync(element, sources)
                                                                .Result;

                               var globalIndices = element.GetGlobalEdgeIndices();

                               // Безопасная вставка в глобальные матрицы
                               for (int i = 0; i < globalIndices.Length; i++)
                               {
                                   int gi = globalIndices[i];

                                   lock (locks[gi])
                                   {
                                       globalRhs[gi] += localVector[i];

                                       for (int j = 0; j < globalIndices.Length; j++)
                                       {
                                           int gj = globalIndices[j];
                                           globalMatrix[gi, gj] += localMatrix[i, j];
                                       }
                                   }
                               }
                           }
                       )
        );

        return (globalMatrix, globalRhs);
    }

    public async Task<Matrix> AssembleStateStiffnessMatrixAsync(Mesh mesh)
    {
        var fakeSources = Array.Empty<CurrentSegment>();
        var (matrix, _) = await AssembleGlobalSystemAsync(mesh, fakeSources);
        return matrix;
    }
}