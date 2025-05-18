using System.Text.Json;

namespace Electromagnetic.Common.Services;

public static class ModelFromJsonLoader
{
    public static async Task<T> LoadOptionsAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<T>(json)!;
    }
}