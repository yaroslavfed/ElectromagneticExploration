using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;

namespace Inverse.SharedCore.Services.InverseService;

public interface IInversionService
{
    /// <summary>
    /// Выполняет один шаг инверсии для восстановления распределения магнитной проницаемости
    /// по наблюдаемым данным магнитного поля, используя компоненты Bx, By, Bz.
    /// <br />
    /// Метод решает нормальное уравнение вида:
    ///     (JT J + λI) delta mu = JT r,
    /// где J — матрица возмущений [3m x n], r — невязка (Bx, By, Bz), λ — коэффициент регуляризации,
    /// delta mu — поправка к модели (значения μ по ячейкам).
    /// </summary>
    /// <param name="mesh">Модель расчётной сетки с ячейками и их геометрией.</param>
    /// <param name="modelValues">
    /// Модельные значения магнитного поля (Bx, By, Bz), полученные в текущей итерации.
    /// </param>
    /// <param name="observedValues">
    /// Наблюдаемые (истинные) значения магнитного поля, в том же формате, что и modelValues.
    /// </param>
    /// <param name="jacobianRaw">
    /// Матрица возмущений [3m x n], отражающая чувствительность компонент магнитного поля по каждой ячейке.
    /// </param>
    /// <param name="parameters">
    /// Текущие значения параметров модели (магнитная проницаемость mu), массив длины n (по ячейкам).
    /// </param>
    /// <param name="options">Параметры инверсии: регуляризация, сходимость и тд.</param>
    /// <param name="iterationNumber">Номер текущей итерации, используется для динамического затухания alfa.</param>
    /// <param name="iterationStagnation">Начались ли стагнация функционала</param>
    /// <returns>Новый вектор параметров модели (mu), длины n, после применения поправки delta mu.</returns>
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