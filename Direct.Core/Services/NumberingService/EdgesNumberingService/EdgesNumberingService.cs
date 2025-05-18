using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.NumberingService.EdgesNumberingService;

/// <summary>
/// Сервис глобальной нумерации ребер для каждого конечного элемента
/// </summary>
public class EdgesNumberingService : IEdgesNumberingService
{
    /// <inheritdoc cref="INumberingService.ConfigureGlobalNumbering"/>
    public async Task ConfigureGlobalNumbering(int nx, int ny, int nz, IList<FiniteElementWithNumerics> finiteElements)
    {
        for (int k = 0, finiteElementIndex = 0; k < nz - 1; k++)
            for (var i = 0; i < ny - 1; i++)
                for (var j = 0; j < nx - 1; j++, finiteElementIndex++)
                {
                    var splitSlices = nx * (ny - 1) + ny * (nx - 1);
                    var sliceCount = nx * ny;

                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        k * (splitSlices + sliceCount) + i * (2 * nx - 1) + j,
                        0,
                        1
                    );
                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        k * (splitSlices + sliceCount) + (i + 1) * (2 * nx - 1) + j,
                        2,
                        3
                    );
                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        (k + 1) * (splitSlices + sliceCount) + i * (2 * nx - 1) + j,
                        4,
                        5
                    );
                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        (k + 1) * (splitSlices + sliceCount) + (i + 1) * (2 * nx - 1) + j,
                        6,
                        7
                    );
                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        k * (splitSlices + sliceCount) + i * (2 * nx - 1) + j + (nx - 1),
                        0,
                        2
                    );
                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        k * (splitSlices + sliceCount) + i * (2 * nx - 1) + j + 1 + (nx - 1),
                        1,
                        3
                    );
                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        (k + 1) * (splitSlices + sliceCount) + i * (2 * nx - 1) + (nx - 1) + j,
                        4,
                        6
                    );
                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        (k + 1) * (splitSlices + sliceCount) + i * (2 * nx - 1) + (nx - 1) + j + 1,
                        5,
                        7
                    );
                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        k * (splitSlices + sliceCount) + splitSlices + j + i * nx,
                        0,
                        4
                    );
                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        k * (splitSlices + sliceCount) + splitSlices + j + i * nx + 1,
                        1,
                        5
                    );
                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        k * (splitSlices + sliceCount) + splitSlices + j + i * nx + nx,
                        2,
                        6
                    );
                    await FillFiniteElementsAsync(
                        finiteElements,
                        finiteElementIndex,
                        k * (splitSlices + sliceCount) + splitSlices + j + i * nx + nx + 1,
                        3,
                        7
                    );
                }
    }

    /// <summary>
    /// Заполнение списка конечных элементов
    /// </summary>
    private static Task FillFiniteElementsAsync(
        IList<FiniteElementWithNumerics> finiteElements,
        int finiteElementIndex,
        int edgeIndex,
        int firstIndex,
        int secondIndex
    )
    {
        finiteElements[finiteElementIndex].Edges.Add(edgeIndex);
        finiteElements[finiteElementIndex]
            .MapNodesEdges.Add(
                new()
                {
                    First = finiteElements[finiteElementIndex].Nodes[firstIndex],
                    Second = finiteElements[finiteElementIndex].Nodes[secondIndex]
                }
            );

        return Task.CompletedTask;
    }
}