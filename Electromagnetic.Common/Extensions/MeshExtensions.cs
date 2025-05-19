using Electromagnetic.Common.Data.Domain;

namespace Electromagnetic.Common.Extensions;

public static class MeshExtensions
{
    /// <summary>
    /// Клонирует сетку, устанавливая одинаковое значение магнитной проницаемости mu для всех ячеек.
    /// </summary>
    public static Mesh CloneWithUniformMu(this Mesh originalMesh, double uniformMu)
    {
        var clonedElements = new List<FiniteElement>(originalMesh.Elements.Count);

        foreach (var originalElement in originalMesh.Elements)
        {
            var clonedEdges = originalElement
                              .Edges
                              .Select(edge => new Edge
                                  {
                                      EdgeIndex = edge.EdgeIndex,
                                      Nodes = edge
                                              .Nodes
                                              .Select(node => new Node
                                                  {
                                                      NodeIndex = node.NodeIndex,
                                                      Coordinate = new Point3D(
                                                          node.Coordinate.X,
                                                          node.Coordinate.Y,
                                                          node.Coordinate.Z
                                                      )
                                                  }
                                              )
                                              .ToList()
                                  }
                              )
                              .ToList();

            clonedElements.Add(new FiniteElement { Edges = clonedEdges, Mu = uniformMu });
        }

        return new Mesh { Elements = clonedElements };
    }
}