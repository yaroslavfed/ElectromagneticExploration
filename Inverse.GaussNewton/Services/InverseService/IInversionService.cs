using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;

namespace Inverse.GaussNewton.Services.InverseService;

public interface IInversionService
{
    /// <summary>
    /// Выполняет один шаг инверсии методом Гаусса–Ньютона для восстановления распределения магнитной проницаемости (μ)
    /// по наблюдаемым данным магнитного поля, используя компонентную (векторную) информацию Bx, By, Bz.
    /// <br />
    /// Метод решает нормальное уравнение вида:
    ///     (Jᵀ J + λI) Δμ = Jᵀ r,
    /// где J — Якобиан [3m x n], r — невязка (Bx, By, Bz), λ — коэффициент регуляризации,
    /// Δμ — поправка к модели (значения μ по ячейкам).
    /// </summary>
    /// <param name="mesh">Модель расчётной сетки с ячейками и их геометрией.</param>
    /// <param name="modelValues">
    /// Модельные значения магнитного поля (Bx, By, Bz), полученные в текущей итерации, вектор длины 3 * m.
    /// Формат: [Bx₀, By₀, Bz₀, Bx₁, By₁, Bz₁, ..., Bxₘ₋₁, Byₘ₋₁, Bzₘ₋₁].
    /// </param>
    /// <param name="observedValues">
    /// Наблюдаемые (истинные) значения магнитного поля, в том же формате, что и modelValues (длина 3 * m).
    /// </param>
    /// <param name="jacobianRaw">
    /// Матрица Якобиана [3m x n], отражающая чувствительность компонент магнитного поля по каждой ячейке.
    /// </param>
    /// <param name="parameters">
    /// Текущие значения параметров модели (магнитная проницаемость μ), массив длины n (по ячейкам).
    /// </param>
    /// <param name="options">Параметры инверсии: регуляризация, сходимость и др.</param>
    /// <param name="iterationNumber">Номер текущей итерации, используется для динамического затухания λ.</param>
    /// <param name="iterationStagnation">Начались ли стагнация функционала</param>
    /// <returns>Новый вектор параметров модели (μ), длины n, после применения поправки Δμ.</returns>
    double[] Invert(
        Mesh mesh,
        double[] modelValues,
        double[] observedValues,
        double[,] jacobianRaw,
        double[] parameters,
        InverseOptions options,
        int iterationNumber,
        bool iterationStagnation
    );
}