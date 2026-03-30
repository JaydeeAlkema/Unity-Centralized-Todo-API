using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnityCentralizedTodoApi.Tests.Helpers;

/// <summary>
/// JsonSerializerOptions that mirror the API's own serialization settings,
/// used when deserializing HTTP responses in integration tests.
/// </summary>
internal static class TestJsonOptions
{
    internal static readonly JsonSerializerOptions Api = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}
