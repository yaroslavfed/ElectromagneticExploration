using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Data.TestSession;
using Electromagnetic.Common.Enums;

namespace Direct.Core.Services.ProblemService;

public interface IProblemService
{
    /// <summary>
    /// Собирает локальную матрицу жёсткости (rot-rot) для заданного конечного элемента.
    /// </summary>
    /// <param name="element">Конечный элемент</param>
    Task<Matrix> AssembleElementStiffnessMatrixAsync(FiniteElement element);

    /// <summary>
    /// Собирает локальный вектор правой части для заданного конечного элемента.
    /// Вклад даётся только от тех сегментов тока, которые попадают внутрь элемента.
    /// </summary>
    /// <param name="element">Конечный элемент</param>
    /// <param name="sources">Список токовых сегментов</param>
    Task<Vector> AssembleElementRightHandVectorAsync(FiniteElement element, IEnumerable<CurrentSegment> sources);

    Task<(Node firstNode, Node secondNode, EDirections direction)> ResolveLocalNodes(
        Edge edge,
        TestSession<Mesh> testSession
    );
}