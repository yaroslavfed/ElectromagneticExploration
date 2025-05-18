using Direct.Core.Services.BasisFunctionProvider;
using Direct.Core.Services.StaticServices.IntegrationHelper;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Data.TestSession;
using Electromagnetic.Common.Enums;
using Electromagnetic.Common.Extensions;

namespace Direct.Core.Services.ProblemService;

/// <inheritdoc cref="IProblemService"/>
public class ProblemService : IProblemService
{
    private readonly IBasisFunctionProvider _basisFunctionProvider;

    public ProblemService(IBasisFunctionProvider basisFunctionProvider)
    {
        _basisFunctionProvider = basisFunctionProvider;
    }

    /// <inheritdoc />
    public Task<Matrix> AssembleElementStiffnessMatrixAsync(FiniteElement element)
    {
        const int edgeCount = 12;
        var localMatrix = new Matrix(edgeCount, edgeCount);

        var integrationPoints = IntegrationHelper.GetIntegrationPoints(element);

        for (int i = 0; i < edgeCount; i++)
        {
            for (int j = i; j < edgeCount; j++)
            {
                double sum = 0.0;

                foreach (var point in integrationPoints)
                {
                    var curlI = _basisFunctionProvider.GetCurl(element, i, point.Position);
                    var curlJ = _basisFunctionProvider.GetCurl(element, j, point.Position);

                    sum += (1.0 / element.Mu) * curlI.Dot(curlJ) * point.Weight;
                    // Console.WriteLine(
                    //     $"i = {i}, j = {j}, Mu = {element.Mu}, curlI.Norm = {curlI.Norm()}, curlJ.Norm = {curlJ.Norm()}, curlI·curlJ = {curlI.Dot(curlJ)}, weight = {point.Weight}, term = {(1.0 / element.Mu) * curlI.Dot(curlJ) * point.Weight}"
                    // );
                }

                localMatrix[i, j] = sum;
                if (i != j)
                    localMatrix[j, i] = sum;
            }
        }

        // var localMin = localMatrix.Min();
        // var localMax = localMatrix.Max();

        // if (localMin < -1 || localMax > 1)
        //     Console.ForegroundColor = ConsoleColor.Red;
        //
        // Console.WriteLine(
        //     $"localMatrix.Size {localMatrix.Rows * localMatrix.Columns}\tlocalMatrix.Min {localMin}\tlocalMatrix.Max {localMax}"
        // );
        //
        // Console.ResetColor();

        return Task.FromResult(localMatrix);
    }

    /// <inheritdoc />
    public Task<Vector> AssembleElementRightHandVectorAsync(FiniteElement element, IEnumerable<CurrentSegment> sources)
    {
        const int edgeCount = 12;
        var localVector = new Vector(edgeCount);

        foreach (var segment in sources)
        {
            if (!element.Contains(segment.Center))
            {
                // Console.WriteLine($"Segment center {segment.Center} is outside element.");
                continue;
            }

            for (int i = 0; i < edgeCount; i++)
            {
                var phi = _basisFunctionProvider.GetValue(element, i, segment.Center); // φ_i(r_k)
                var dot = segment.Direction.Dot(phi);
                localVector[i] += dot * segment.Current;
            }
        }

        // Console.WriteLine(
        //     $"localVector.Size {localVector.Size}\tlocalVector.Min {localVector.Min()}\tlocalVector.Max {localVector.Max()}"
        // );

        return Task.FromResult(localVector);
    }

    /// <inheritdoc />
    public async Task<(Node firstNode, Node secondNode, EDirections direction)> ResolveLocalNodes(
        Edge edge,
        TestSession<Mesh> testSession
    )
    {
        var finiteElementIndex = await edge.FiniteElementIndexByEdges(testSession.Mesh);
        var localFiniteElement = testSession.Mesh.Elements[finiteElementIndex];

        var edgeIndex = await edge.ResolveLocal(localFiniteElement);
        var firstNode = localFiniteElement.Edges[edgeIndex].Nodes[0];
        var secondNode = localFiniteElement.Edges[edgeIndex].Nodes[1];

        var firstNodeIndex = await firstNode.ResolveLocal(localFiniteElement);
        var secondNodeIndex = await secondNode.ResolveLocal(localFiniteElement);

        var nodesList = localFiniteElement
                        .Edges
                        .SelectMany(item => item.Nodes)
                        .DistinctBy(node => node.NodeIndex)
                        .OrderBy(node => node.NodeIndex)
                        .ToList();

        var localFirstNode = new Node
        {
            Coordinate = new()
            {
                X = nodesList[firstNodeIndex].Coordinate.X,
                Y = nodesList[firstNodeIndex].Coordinate.Y,
                Z = nodesList[firstNodeIndex].Coordinate.Z
            }
        };

        var localSecondNode = new Node
        {
            Coordinate = new()
            {
                X = nodesList[secondNodeIndex].Coordinate.X,
                Y = nodesList[secondNodeIndex].Coordinate.Y,
                Z = nodesList[secondNodeIndex].Coordinate.Z
            }
        };

        var stepX = Math.Abs(localFirstNode.Coordinate.X - localSecondNode.Coordinate.X);
        var stepY = Math.Abs(localFirstNode.Coordinate.Y - localSecondNode.Coordinate.Y);
        var stepZ = Math.Abs(localFirstNode.Coordinate.Z - localSecondNode.Coordinate.Z);

        var direction = EDirections.Ox;
        if (stepX > 0)
            direction = EDirections.Ox;
        else if (stepY > 0)
            direction = EDirections.Oy;
        else if (stepZ > 0)
            direction = EDirections.Oz;

        return (firstNode, secondNode, direction);
    }

    private static Task<Node> CalculateNodeAsync(EDirections direction, (Node firstNode, Node secondNode) nodesPair)
    {
        var result = direction switch
        {
            EDirections.Ox => nodesPair.firstNode with
            {
                Coordinate = nodesPair.firstNode.Coordinate with
                {
                    X = (nodesPair.firstNode.Coordinate.X + nodesPair.secondNode.Coordinate.X) / 2.0
                }
            },
            EDirections.Oy => nodesPair.firstNode with
            {
                Coordinate = nodesPair.firstNode.Coordinate with
                {
                    Y = (nodesPair.firstNode.Coordinate.Y + nodesPair.secondNode.Coordinate.Y) / 2.0
                }
            },
            EDirections.Oz => nodesPair.firstNode with
            {
                Coordinate = nodesPair.firstNode.Coordinate with
                {
                    Z = (nodesPair.firstNode.Coordinate.Z + nodesPair.secondNode.Coordinate.Z) / 2.0
                }
            },
            _ => throw new NotImplementedException()
        };

        return Task.FromResult(result);
    }
}