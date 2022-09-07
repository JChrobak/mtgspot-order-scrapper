using System.Text.RegularExpressions;

namespace MtgSpotOrdersScrapper;

public class Application
{
    private readonly HttpClientWrapper _httpClient;
    private readonly ConfigurationManager _configurationManager;
    private ConfigurationAppSettings AppSettings => _configurationManager.AppSettings;

    public Application()
    {
        _configurationManager = new ConfigurationManager();
        _httpClient = new HttpClientWrapper(_configurationManager);
    }

    public async Task OutputUserCollection()
    {
        var collection = await GetUserCollection();
        Console.WriteLine($"Started saving collection to file: {AppSettings.OutputFileName}");
        await using var streamWriter = new StreamWriter(path: AppSettings.OutputFileName, append: false);
        foreach (var item in collection)
        {
            var itemName = Regex.Replace(item.Name ?? string.Empty, AppSettings.ItemNameReplacePattern, "");
            await streamWriter.WriteLineAsync($"{item.Quantity} {itemName}");
        }

        Console.WriteLine("Finished saving collection!");
    }

    private async Task<List<ItemDto>> GetUserCollection()
    {
        if (!await _httpClient.Login())
            return new List<ItemDto>();
        var orders = await _httpClient.GetOrders();
        var collection = new List<ItemDto>();
        foreach (var o in orders)
        {
            var items = await _httpClient.GetOrderItems(o);
            collection.AddRange(items.Where(ItemIsNotExcluded));
        }

        Console.WriteLine("Finished fetching collection!");
        return collection;
    }

    private bool ItemIsNotExcluded(ItemDto item)
    {
        return !string.IsNullOrWhiteSpace(item.CategoryName) &&
               !AppSettings.ExcludedItemCategories.Contains(item.CategoryName) &&
               !string.IsNullOrWhiteSpace(item.Rarity) &&
               !AppSettings.ExcludedItemRarities.Contains(item.Rarity) &&
               !string.IsNullOrWhiteSpace(item.Name) &&
               !AppSettings.ExcludedItemNames.Contains(item.Name);
    }
}