using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;
using MathNet.Numerics.LinearAlgebra;

// ReSharper disable InconsistentNaming

namespace Inverse.GaussNewton.Services.InverseService;

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
        out double effectiveLambda
    )
    {
        int n = parameters.Length;

        // Вычисляем вектор невязки r = eps_observed - eps_model = невязка между измерениями и модельным откликом
        var residual = Vector<double>.Build.DenseOfEnumerable(
            observedValues.Zip(modelValues, (obs, calc) => obs - calc)
        );
        
        var J = Matrix<double>.Build.DenseOfArray(jacobianRaw);

        // Вычисляем A = J^T * J и b = J^T * r
        var JT = J.Transpose();
        var JTJ = JT * J;
        var JTr = JT * residual;

        // Вычисляем лямбду с учётом динамического затухания
        double baseLambda = options.Lambda;
        effectiveLambda = baseLambda;

        if (options.AutoAdjustRegularization)
        {
            effectiveLambda = baseLambda * Math.Pow(options.LambdaDecay, iterationNumber);
            effectiveLambda = Math.Max(effectiveLambda, options.MinLambda);
        }

        // Добавляем регуляризацию на величину mu (Тихонов 1)
        if (options.UseTikhonovFirstOrder)
        {
            for (int i = 0; i < n; i++)
                JTJ[i, i] += effectiveLambda;
        }

        // Добавляем сглаживающую регуляризацию по кривизне mu (Тихонов 2)
        if (options.UseTikhonovSecondOrder)
        {
            double gamma = effectiveLambda * options.SecondOrderRegularizationLambdaMultiplier;
            AddTikhonovSecondOrderRegularization(JTJ, mesh, gamma);
        }

        // Решаем СЛАУ
        var delta = JTJ.Solve(JTr);

        // Обновляем параметры модели
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