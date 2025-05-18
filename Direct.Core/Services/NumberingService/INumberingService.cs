using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.NumberingService;

/// <summary>
/// Сервис задания глобальной нумерации
/// </summary>
public interface INumberingService
{
    /// <summary>
    /// Метод глобальной нумерации
    /// </summary>
    /// <param name="nx">Количество узлов по OX</param>
    /// <param name="ny">Количество узлов по OY</param>
    /// <param name="nz">Количество узлов по OZ</param>
    /// <param name="finiteElements">Список конечных элементов</param>
    Task ConfigureGlobalNumbering(int nx, int ny, int nz, IList<FiniteElementWithNumerics> finiteElements);
}