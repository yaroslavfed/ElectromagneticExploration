using Electromagnetic.Common.Data.Domain;

namespace Electromagnetic.Common.Extensions;

public static class EdgeExtensions
{
    public static Task<int> ResolveLocal(this Edge edge, FiniteElement element)
    {
        var edges = element.Edges;
        for (var i = 0; i < 12; i++)
            if (edge.EdgeIndex == edges[i].EdgeIndex)
                return Task.FromResult(i);

        return Task.FromResult(0);
    }

    public static Task<int> FiniteElementIndexByEdges(this Edge edge, Mesh strata)
    {
        for (var i = 0; i < strata.Elements.Count; i++)
        {
            for (var j = 0; j < 12; j++)
            {
                if (strata.Elements[i].Edges[j].EdgeIndex == edge.EdgeIndex)
                {
                    return Task.FromResult(i);
                }
            }
        }

        return Task.FromResult(0);
    }
}