using MathNet.Numerics.LinearAlgebra;

namespace Electromagnetic.Common.Data.Domain;

public record Matrix
{
    private readonly double[,] _data;

    public int Rows { get; }

    public int Columns { get; }

    public Matrix(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _data = new double[rows, columns];
    }

    public double this[int i, int j]
    {
        get => _data[i, j];
        set => _data[i, j] = value;
    }

    public double Min()
    {
        if (_data == null)
            throw new InvalidOperationException("Массив не инициализирован.");

        int rows = _data.GetLength(0);
        int cols = _data.GetLength(1);

        if (rows == 0 || cols == 0)
            throw new InvalidOperationException("Массив пуст.");

        // Проверяем наличие NaN
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (double.IsNaN(_data[i, j]))
                    throw new InvalidOperationException("Массив содержит значения NaN.");
            }
        }

        double min = _data[0, 0];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (_data[i, j] < min)
                    min = _data[i, j];
            }
        }

        return min;
    }

    public double Max()
    {
        if (_data == null)
            throw new InvalidOperationException("Массив не инициализирован.");

        int rows = _data.GetLength(0);
        int cols = _data.GetLength(1);

        if (rows == 0 || cols == 0)
            throw new InvalidOperationException("Массив пуст.");

        // Проверяем наличие NaN
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (double.IsNaN(_data[i, j]))
                    throw new InvalidOperationException("Массив содержит значения NaN.");
            }
        }

        double max = _data[0, 0];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (_data[i, j] > max)
                    max = _data[i, j];
            }
        }

        return max;
    }

    public void Add(Matrix other)
    {
        if (other.Rows != Rows || other.Columns != Columns)
            throw new ArgumentException("Matrix dimensions do not match");

        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Columns; j++)
                _data[i, j] += other[i, j];
    }

    public Vector Multiply(Vector vector)
    {
        if (vector.Size != Columns)
            throw new ArgumentException("Matrix and vector dimensions do not match");

        Vector result = new Vector(Rows);
        for (int i = 0; i < Rows; i++)
        {
            double sum = 0;
            for (int j = 0; j < Columns; j++)
                sum += _data[i, j] * vector[j];

            result[i] = sum;
        }

        return result;
    }

    public override string ToString()
    {
        var lines = new List<string>();
        for (int i = 0; i < Rows; i++)
        {
            var row = Enumerable.Range(0, Columns).Select(j => _data[i, j].ToString("0.###")).ToArray();
            lines.Add("[" + string.Join(", ", row) + "]");
        }

        return string.Join("\n", lines);
    }

    public void Assemble(Matrix local, int[] globalIndices)
    {
        for (int i = 0; i < local.Rows; i++)
        {
            for (int j = 0; j < local.Columns; j++)
            {
                int row = globalIndices[i];
                int col = globalIndices[j];
                this[row, col] += local[i, j];
            }
        }
    }

    public void ClearRow(int row)
    {
        for (int j = 0; j < Columns; j++)
            this[row, j] = 0.0;
    }

    public void ClearColumn(int col)
    {
        for (int i = 0; i < Rows; i++)
            this[i, col] = 0.0;
    }

    /// <summary>
    /// Преобразование в MathNet-матрицу
    /// </summary>
    /// <returns>MathNet-матрица</returns>
    public Matrix<double> ToMathNet()
    {
        return Matrix<double>.Build.Dense(Rows, Columns, (i, j) => _data[i, j]);
    }
}