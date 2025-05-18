namespace Electromagnetic.Common.Data.Domain;

public record CurrentSource
{
    /// <summary>
    /// Начало отрезка
    /// </summary>
    public required Point3D Start { get; set; }

    /// <summary>
    /// Конец отрезка
    /// </summary>
    public required Point3D End { get; set; }

    /// <summary>
    /// Величина тока в амперах
    /// </summary>
    public double Amperage { get; set; }

    /// <summary>
    /// Число разбиений на сегменты
    /// </summary>
    public int Segments { get; set; } = 10;
}