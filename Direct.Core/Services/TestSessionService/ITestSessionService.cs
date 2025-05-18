using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Data.TestSession;

namespace Direct.Core.Services.TestSessionService;

/// <summary>
/// Сервис создания сессии тестирования
/// </summary>
public interface ITestSessionService
{
    /// <summary>
    /// Создаем сессию тестирования
    /// </summary>
    /// <remarks>Для использования с внешними в API</remarks>
    /// <returns>Сессия тестирования расчётной области</returns>
    Task<TestSession<Mesh>> CreateTestSessionAsync(TestSession testSession);
}