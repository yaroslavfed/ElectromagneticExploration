using Electromagnetic.Common.Enums;

namespace Electromagnetic.Common.Data.InputModels;

/// <summary>
/// Набор дополнительных параметров тестирования
/// </summary>
public record AdditionalParameters
{
    public double Mu { get; init; }

    public double Gamma { get; init; }

    /// <summary>
    /// Номер краевого условия
    /// </summary>
    public EBoundaryConditions BoundaryCondition { get; init; }
}