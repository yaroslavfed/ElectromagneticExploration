using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;
using MathNet.Numerics.LinearAlgebra;

// ReSharper disable InconsistentNaming

namespace Inverse.SharedCore.Services.InverseService;

public class InversionService : IInversionService
{
    /// <inheritdoc />
    public double[] Invert(
        Mesh mesh,
        double[] modelValues,
        double[] observedValues,
        double[,] jacobianRaw,
        double[] parameters,
        InverseOptions options,
        int iterationNumber,
        bool iterationStagnation
    )
    {
        int n = parameters.Length;

        var residual = Vector<double>.Build.DenseOfEnumerable(
            observedValues.Zip(modelValues, (obs, calc) => obs - calc)
        );

        var J = Matrix<double>.Build.DenseOfArray(jacobianRaw);

        // J^T * J и J^T * r
        var JT = J.Transpose();
        var JTJ = JT * J;
        var JTr = JT * residual;

        // Динамическая регуляризация
        var baseLambda = options.Lambda;
        double effectiveLambda;
        if (options.AutoAdjustRegularization)
        {
            effectiveLambda = iterationStagnation
                ? Math.Min(baseLambda / Math.Pow(options.LambdaDecay, iterationNumber), options.MaxLambda)
                : Math.Max(baseLambda * Math.Pow(options.LambdaDecay, iterationNumber), options.MinLambda);
        }
        else
            effectiveLambda = baseLambda;

        // Регуляризация Тихонова 1-го порядка
        if (options.UseTikhonovFirstOrder)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Using Tikhonov first order: alfa = {effectiveLambda}");
            for (int i = 0; i < n; i++)
                JTJ[i, i] += effectiveLambda;
            Console.ResetColor();
        }

        // Регуляризация Тихонова 2-го порядка
        if (options.UseTikhonovSecondOrder)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            double gamma = effectiveLambda * options.SecondOrderRegularizationLambdaMultiplier;
            Console.WriteLine($"Using Tikhonov second order: gamma = {gamma}");
            AddTikhonovSecondOrderRegularization(JTJ, mesh, gamma);
            Console.ResetColor();
        }

        var delta = JTJ.Solve(JTr);
        return parameters.Zip(delta, (p, d) => p + d).ToArray();
    }

    /// <summary>
    /// Добавляет регуляризацию второго порядка G * mu
    /// </summary>
    /// <param name="A">Матрица A = J^T J, в которую добавляется G</param>
    /// <param name="mesh">Сетка с элементами и их геометрией</param>
    /// <param name="gamma">Коэффициент регуляризации γ</param>
    private void AddTikhonovSecondOrderRegularization(Matrix<double> A, Mesh mesh, double gamma)
    {
        var elements = mesh.Elements;
        int n = elements.Count;

        for (int i = 0; i < n; i++)
        {
            var current = elements[i];

            // Найдём соседей по геометрии (смежные блоки по грани)
            var neighbors = new List<int>();
            for (int j = 0; j < n; j++)
            {
                if (i == j) continue;

                var other = elements[j];
                if (AreElementsAdjacent(current, other))
                {
                    neighbors.Add(j);
                }
            }

            int degree = neighbors.Count;
            A[i, i] += gamma * degree;

            foreach (var j in neighbors)
            {
                A[i, j] += -gamma;
            }
        }
    }

    /// <summary>
    /// Проверка, являются ли два элемента соседними (имеют общую грань)
    /// </summary>
    private static bool AreElementsAdjacent(FiniteElement a, FiniteElement b)
    {
        var aEdges = a.Edges.Select(e => e.EdgeIndex).ToHashSet();
        var bEdges = b.Edges.Select(e => e.EdgeIndex).ToHashSet();

        return aEdges.Overlaps(bEdges);
    }
}