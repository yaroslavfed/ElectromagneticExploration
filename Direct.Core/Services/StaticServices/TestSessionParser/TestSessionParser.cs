using System.Text.Json;
using System.Text.Json.Serialization;
using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.StaticServices.TestSessionParser;

public static class TestSessionParser
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public static TestSession ParseFromJson(string json)
    {
        try
        {
            var result = JsonSerializer.Deserialize<TestSession>(json, s_options);

            if (result == null)
            {
                throw new JsonException("Deserialization returned null");
            }

            return result;
        } catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON format", nameof(json), ex);
        }
    }
}