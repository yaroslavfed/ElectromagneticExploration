using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.MeshService;

/// <summary>
/// Сервис расчёта сетки
/// </summary>
public interface IMeshService
{
    /// <summary>
    /// Генерация сетки расчётной области
    /// </summary>
    /// <returns></returns>
    Task<Mesh> GenerateMeshAsync(Axis axis);

    /// <summary>
    /// Получение входных данных из внешнего источника
    /// </summary>
    /// <remarks>Для использования с API</remarks>
    /// <param name="testSession">Параметры проводимой расчётной сессии</param>
    /// <returns>Параметры проводимого теста</returns>
    Task<Axis> GenerateTestConfiguration(TestSession testSession);

    /// <summary>
    /// Добавление физического параметра в КЭ
    /// </summary>
    /// <param name="mesh">Сетка расчётной области</param>
    /// <param name="meshModel">Параметры расчётной обрасти</param>
    public Task AssignMuesAsync(Mesh mesh, Axis meshModel);
}