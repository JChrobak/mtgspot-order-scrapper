using Microsoft.Extensions.Configuration;

namespace MtgSpotOrdersScrapper;

public class ConfigurationManager
{
    private IConfiguration _configuration;

    public ConfigurationCredentials Credentials =>
        _configuration.GetSection(nameof(Credentials)).Get<ConfigurationCredentials>();

    public ConfigurationHttpClientSettings HttpClientSettings =>
        _configuration.GetSection(nameof(HttpClientSettings)).Get<ConfigurationHttpClientSettings>();

    public ConfigurationAppSettings AppSettings =>
        _configuration.GetSection(nameof(AppSettings)).Get<ConfigurationAppSettings>();

    public ConfigurationManager()
    {
        _configuration = BuildConfig();
    }

    private static IConfiguration BuildConfig()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"config.json");
        return builder.Build();
    }
}

public class ConfigurationCredentials
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class ConfigurationHttpClientSettings
{
    public string ClientId { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
    public string LoginUrl { get; set; } = null!;
    public string OrdersUrl { get; set; } = null!;
}

public class ConfigurationAppSettings
{
    public string ItemNameReplacePattern { get; set; } = null!;
    public List<string> ExcludedItemCategories { get; set; } = null!;
    public List<string> ExcludedItemRarities { get; set; } = null!;
    public List<string> ExcludedItemNames { get; set; } = null!;
    public string OutputFileName { get; set; } = null!;
}