namespace Electromagnetic.Common.Extensions;

public static class CommonExtensions
{
    public static IReadOnlyList<IReadOnlyList<TData>> ArrayToList<TData>(this TData[,] array)
    {
        var result = new List<List<TData>>();
        for (var i = 0; i < Math.Sqrt(array.Length); i++)
        {
            var line = new List<TData>();
            for (var j = 0; j < Math.Sqrt(array.Length); j++) line.Add(array[i, j]);

            result.Add(line);
        }

        return result;
    }
}