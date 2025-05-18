using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.NumberingService.NodesNumberingService;

/// <summary>
/// Глобальная нумерация узлов для каждого конечного элемента
/// </summary>
public class NodesNumberingService : INodesNumberingService
{
    /// <inheritdoc cref="INumberingService.ConfigureGlobalNumbering"/>
    public Task ConfigureGlobalNumbering(int nx, int ny, int nz, IList<FiniteElementWithNumerics> finiteElements)
    {
        for (int i = 0, k = 0, m = 1; i < nz - 1; i++, m += nx)
            for (var j = 0; j < ny - 1; j++, m++)
                for (var p = 0; p < nx - 1; p++, m++, k++)
                {
                    finiteElements[k].Nodes.Add(m - 1);
                    finiteElements[k].Nodes.Add(m);
                    finiteElements[k].Nodes.Add(m + nx - 1);
                    finiteElements[k].Nodes.Add(m + nx);
                    finiteElements[k].Nodes.Add(m + ny * nx - 1);
                    finiteElements[k].Nodes.Add(m + ny * nx);
                    finiteElements[k].Nodes.Add(m + nx + ny * nx - 1);
                    finiteElements[k].Nodes.Add(m + nx + ny * nx);
                }

        return Task.CompletedTask;
    }
}