using Electromagnetic.Common.Data.Domain;

namespace Electromagnetic.Common.Services;

public static class MeshNoiseAdder
{
    private static readonly Random s_random = new();

    // Метод для добавления шума в плотности ячеек
    public static void AddGaussianNoise(InverseMesh inverseMesh, double percentage)
    {
        // Проверка на допустимость процента
        if (percentage is < 0 or > 100)
        {
            throw new ArgumentException("Процент должен быть в диапазоне от 0 до 100.");
        }

        // Преобразуем процент в коэффициент (например, 10% -> 0.1)
        double noiseFactor = percentage / 100.0;

        // Проходим по всем ячейкам и добавляем гауссовский шум
        foreach (var cell in inverseMesh.Cells)
        {
            // Генерация случайного значения шума из нормального распределения (Gaussian)
            double gaussianNoise = s_random.NextDouble() * 2.0 - 1.0; // От -1 до 1
            gaussianNoise *= noiseFactor;                             // Умножаем на фактор шума для нужного процента

            // Модифицируем плотность с учетом шума
            double newDensity = cell.Mu * (1 + gaussianNoise);

            // Применяем новое значение плотности
            // Если необходимо, можно ограничить плотность положительным значением
            cell.Mu = Math.Max(newDensity, 0);
        }
    }
}