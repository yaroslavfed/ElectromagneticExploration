using System.Text.Json.Serialization;

namespace Electromagnetic.Common.Models;

/// <summary>
/// Параметры настройки метода Гаусса–Ньютона для решения обратной задачи.
/// Управляют регуляризацией, критериями останова и адаптацией сглаживания.
/// </summary>
public record InverseOptions
{
    /// <summary>
    /// Лимит функционала, если отношение между текущим функционалом на итерации меньше во сколько раз, чем первоначальный функционал - инверсия завершается
    /// </summary>
    public double FunctionalThreshold { get; set; }

    /// <summary>
    /// Использование лимита по времени
    /// </summary>
    public bool UseTimeThreshold { get; set; }

    /// <summary>
    /// Порог выполнения задачи, если задача решается дольше этого значения - инверсия завершается
    /// </summary>
    public double TimeThreshold { get; set; }

    /// <summary>
    /// Начальный коэффициент регуляризации (амплитудная регуляризация).
    /// </summary>
    public double Lambda { get; set; }

    /// <summary>
    /// Минимально допустимое значение регуляризации.
    /// Используется для ограничения автоматического уменьшения λ.
    /// </summary>
    public double MinLambda { get; set; }

    /// <summary>
    /// Максимально допустимое значение регуляризации.
    /// Используется для ограничения автоматического увеличения λ.
    /// </summary>
    public double MaxLambda { get; set; }

    /// <summary>
    /// Коэффициент уменьшения λ на каждой итерации.
    /// Например, 0.7 означает уменьшение на 30% за итерацию.
    /// </summary>
    public double LambdaDecay { get; set; }

    /// <summary>
    /// Использовать регуляризацию первого порядка (амплитудную).
    /// Увеличивает диагональные элементы матрицы нормальных уравнений.
    /// </summary>
    public bool UseTikhonovFirstOrder { get; set; }

    /// <summary>
    /// Использовать регуляризацию второго порядка (сглаживающую).
    /// Минимизирует кривизну плотностной модели.
    /// </summary>
    public bool UseTikhonovSecondOrder { get; set; }

    /// <summary>
    /// Множитель для силы сглаживающей регуляризации второго порядка.
    /// Применяется к λ при добавлении второго порядка.
    /// </summary>
    public double SecondOrderRegularizationLambdaMultiplier { get; set; }

    /// <summary>
    /// Активировать автоматическую настройку регуляризации (уменьшение λ по итерациям).
    /// </summary>
    public bool AutoAdjustRegularization { get; set; }

    /// <summary>
    /// Относительное изменение функционала.
    /// </summary>
    public double RelativeTolerance { get; set; }

    /// <summary>
    /// Максимально допустимое число итераций метода адаптивной инверсии.
    /// Защищает от вечных циклов при плохой сходимости.
    /// </summary>
    public int MaxIterations { get; set; }

    /// <summary>
    /// Порог функционала для активации сглаживающей регуляризации.
    /// Если функционал падает ниже этого значения — включается сглаживание.
    /// Может устанавливаться динамически.
    /// </summary>
    public double SmoothingActivationThreshold { get; set; }

    /// <summary>
    /// Доля от начального функционала для автоматического расчёта порога активации сглаживания.
    /// Например, 0.01 означает активацию при уменьшении функционала в 100 раз.
    /// </summary>
    public double DynamicSmoothingActivationFraction { get; set; }

    /// <summary>
    /// Порог на производную (градиент) плотностей для активации второго порядка регуляризации.
    /// Используется в AddTikhonovSecondOrderRegularization.
    /// </summary>
    public double GradientThreshold { get; set; }

    /// <summary>
    /// Порог отключения второй регуляризации при замедлении убывания функционала.
    /// </summary>
    public double SmoothingDisableThreshold { get; init; }

    /// <summary>
    /// Минимальное число итераций, в течение которых сглаживание второго порядка должно быть включено.
    /// </summary>
    public int MinSmoothingIterations { get; init; }

    /// <summary>
    /// Внутреннее поле для отслеживания номера итерации включения сглаживания.
    /// </summary>
    [JsonIgnore]
    public int? SmoothingStartIteration { get; set; }

    /// <summary>
    /// Допустимый рост функционала прежде чем считать это отклонением.
    /// </summary>
    public double FunctionalGrowthTolerance { get; init; }
}