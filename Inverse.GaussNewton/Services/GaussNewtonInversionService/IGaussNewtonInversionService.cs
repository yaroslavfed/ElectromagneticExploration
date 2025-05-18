using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;

namespace Inverse.GaussNewton.Services.GaussNewtonInversionService;

public interface IGaussNewtonInversionService
{
    /// <summary>
    /// Запуск инверсии
    /// </summary>
    /// <param name="trueModelValues">Истинные значения с сенсоров</param>
    /// <param name="sources">Источник тока</param>
    /// <param name="sensors">Сенсоры на поверхности</param>
    /// <param name="primaryField">Поле первичых токов</param>
    /// <param name="baseMu">Магнитная проницаемость среды</param>
    /// <param name="initialMesh">Первоначальная сетка для инверсии</param>
    /// <param name="inversionOptions">Параметры инверсии</param>
    /// <param name="refinementOptions">Параметры адаптации сетки</param>
    Task AdaptiveInvertAsync(
        IReadOnlyList<FieldSample> trueModelValues,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<FieldSample> primaryField,
        double baseMu,
        Mesh initialMesh,
        InverseOptions inversionOptions,
        MeshRefinementOptions refinementOptions
    );
}