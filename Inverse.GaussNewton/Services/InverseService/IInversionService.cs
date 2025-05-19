using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;

namespace Inverse.GaussNewton.Services.InverseService;

public interface IInversionService
{
    /// <summary>
    /// Решает обратную задачу методом Гаусса–Ньютона с регуляризацией первого и второго порядка.
    /// </summary>
    /// <param name="modelValues">Значения аномалий от текущей модели.</param>
    /// <param name="observedValues">Наблюдённые значения.</param>
    /// <param name="jacobianRaw">Матрица Якобиана (m x n).</param>
    /// <param name="parameters">Текущие параметры модели (мю).</param>
    /// <param name="options">Параметры инверсии.</param>
    /// <param name="iterationNumber">Номер текущей итерации.</param>
    /// <param name="effectiveLambda">Фактическое значение регуляризации (λ), применённое на итерации.</param>
    /// <returns>Обновлённый массив параметров модели.</returns>
    double[] Invert(
        Mesh mesh,
        double[] modelValues,
        double[] observedValues,
        double[,] jacobian,
        double[] parameters,
        InverseOptions options,
        int iterationNumber,
        out double effectiveLambda
    );
}