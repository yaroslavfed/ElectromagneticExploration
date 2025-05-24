using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.DirectTaskService;

public interface IDirectTaskService
{
    /// <summary>
    /// Расчет прямой задачи с учетом первичных токов
    /// </summary>
    /// <param name="mesh">Сетка конечных элементов</param>
    /// <param name="sensors">Сетки приемников</param>
    /// <param name="sources">Сетка источников</param>
    /// <returns>Сетку приемников с измеренными компонентами B</returns>
    Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources
    );

    /// <summary>
    /// Расчет прямой задачи без учета первичных токов
    /// </summary>
    /// <param name="mesh">Сетка конечных элементов</param>
    /// <param name="sensors">Сетки приемников</param>
    /// <param name="sources">Сетка источников</param>
    /// <param name="primaryField">Измерения полученные на однородной среде</param>
    /// <returns>Сетку приемников с измеренными компонентами B</returns>
    Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<FieldSample> primaryField
    );

    /// <summary>
    /// Расчет прямой задачи с учетом первичных токов
    /// </summary>
    /// <param name="testSessionParameters">Параметры среды</param>
    /// <param name="showPlot">Рисовать ли график сетки</param>
    /// <returns>Сетку приемников с измеренными компонентами B</returns>
    Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(TestSession testSessionParameters, bool showPlot = true);

    /// <summary>
    /// Расчет прямой задачи без учета первичных токов
    /// </summary>
    /// <param name="testSessionParameters">Параметры среды</param>
    /// <param name="primaryField">Измерения полученные на однородной среде</param>
    /// <param name="showPlot">Рисовать ли график сетки</param>
    /// <returns>Сетку приемников с измеренными компонентами B</returns>
    Task<IReadOnlyList<FieldSample>> CalculateDirectTaskAsync(
        TestSession testSessionParameters,
        IReadOnlyList<FieldSample> primaryField,
        bool showPlot = true
    );

    Task<PrecomputedStiffness> GetFixedStiffnessMatrixAsync(Mesh mesh);

    Task<IReadOnlyList<FieldSample>> CalculateFixedDirectTaskAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        IReadOnlyList<FieldSample> primaryField,
        PrecomputedStiffness stiffness
    );
}