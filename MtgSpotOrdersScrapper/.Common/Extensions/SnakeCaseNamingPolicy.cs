using System.Text.Json;

namespace MtgSpotOrdersScrapper.Common.Extensions;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        return ToSnakeCase(name);
    }

    private static string ToSnakeCase(string name)
    {
        return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x)
            ? "_" + x
            : x.ToString()
        )).ToLower();
    }
}