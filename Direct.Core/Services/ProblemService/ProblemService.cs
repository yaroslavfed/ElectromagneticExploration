using Direct.Core.Services.BasisFunctionProvider;
using Direct.Core.Services.StaticServices.IntegrationHelper;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Data.TestSession;
using Electromagnetic.Common.Enums;
using Electromagnetic.Common.Extensions;

namespace Direct.Core.Services.ProblemService;

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
        int pointCount = integrationPoints.Count;

        // Предвычисление всех роторов
        var curlCache = new Vector3D[edgeCount, pointCount];
        for (int i = 0; i < edgeCount; i++)
        {
            for (int q = 0; q < pointCount; q++)
            {
                curlCache[i, q] = _basisFunctionProvider.GetCurl(element, i, integrationPoints[q].Position);
            }
        }

        for (int i = 0; i < edgeCount; i++)
        {
            for (int j = i; j < edgeCount; j++)
            {
                double sum = 0.0;

                for (int q = 0; q < pointCount; q++)
                {
                    sum += (1.0 / element.Mu) * curlCache[i, q].Dot(curlCache[j, q]) * integrationPoints[q].Weight;
                }

                localMatrix[i, j] = sum;
                if (i != j)
                    localMatrix[j, i] = sum;
            }
        }

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
                continue;
            }

            for (int i = 0; i < edgeCount; i++)
            {
                var phi = _basisFunctionProvider.GetValue(element, i, segment.Center);
                var dot = segment.Direction.Dot(phi);
                localVector[i] += dot * segment.Current;
            }
        }

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
}