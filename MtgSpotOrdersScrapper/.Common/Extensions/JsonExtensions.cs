using System.Net.Http.Json;
using System.Text.Json;

namespace MtgSpotOrdersScrapper.Common.Extensions;

public static class JsonExtensions
{
    private static JsonSerializerOptions Options => new()
    {
        PropertyNamingPolicy = new SnakeCaseNamingPolicy()
    };
    public static Task<T?> DeserializeContentAsync<T>(this HttpResponseMessage response)
    {
        return response.Content.ReadFromJsonAsync<T>(Options);
    }
}