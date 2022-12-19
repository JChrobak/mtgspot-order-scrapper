using System.Text.RegularExpressions;
using MtgSpotOrdersScrapper.Common;

namespace MtgSpotOrdersScrapper;

// ReSharper disable once ClassNeverInstantiated.Global
public class Application : ConsoleAppBase
{
    private readonly MtgSpotService _mtgSpotHttpClient;
    private readonly ConfigurationManager _configurationManager;
    private ConfigurationAppSettings AppSettings => _configurationManager.AppSettings;

    public Application()
    {
        _configurationManager = new ConfigurationManager();
        _mtgSpotHttpClient = new MtgSpotService(_configurationManager);
    }

    [Command("full-collection")]
    // ReSharper disable once UnusedMember.Global
    public async Task OutputUserCollection(
        [Option("-u", "username")] string? username = null,
        [Option("-p", "password")] string? password = null)
    {
        var loginResult = await _mtgSpotHttpClient.Login(username, password);
        if (loginResult.IsFailure)
        {
            await Console.Error.WriteLineAsync(loginResult.ErrorMessage);
            return;
        }

        await Console.Out.WriteLineAsync("Successfully logged in!");

        var collectionResult = await GetUserCollection();
        if (collectionResult.IsFailure)
        {
            await Console.Error.WriteLineAsync(collectionResult.ErrorMessage);
            return;
        }

        var collection = collectionResult.Data!;

        await SaveCollectionToFile(collection);
    }

    private async Task SaveCollectionToFile(List<ItemDto> collection)
    {
        Console.WriteLine($"Started saving collection to file: {AppSettings.OutputFileName}");
        await using var streamWriter = new StreamWriter(path: AppSettings.OutputFileName, append: false);
        foreach (var item in collection)
        {
            var itemName = Regex.Replace(item.Name ?? string.Empty, AppSettings.ItemNameReplacePattern, "");
            await streamWriter.WriteLineAsync($"{item.Quantity} {itemName}");
        }

        Console.WriteLine("Finished saving collection!");
    }

    private async Task<IResult<List<ItemDto>>> GetUserCollection()
    {
        var ordersResult = await _mtgSpotHttpClient.GetOrders();
        if (ordersResult.IsFailure)
            return ordersResult.MapError<List<ItemDto>>();
        var orders = ordersResult.Data!;
        await Console.Out.WriteLineAsync($"Successfully fetched {orders.Count} orders.");
        var collection = new List<ItemDto>();

        foreach (var o in NotExcludedOrders(orders))
        {
            var itemsResult = await _mtgSpotHttpClient.GetOrderItems(o);
            if (itemsResult.IsFailure)
                return itemsResult.MapError<List<ItemDto>>();
            var items = itemsResult.Data!;
            await Console.Out.WriteLineAsync($"Successfully fetched {items.Count} items for order {o.IdOrder}");
            collection.AddRange(items.Where(ItemIsNotExcluded));
        }

        Console.WriteLine("Finished fetching collection!");
        return collection.Success();
    }

    private IEnumerable<OrderDto> NotExcludedOrders(List<OrderDto> orders)
    {
        return orders.Where(o =>
            _configurationManager.AppSettings.ExcludedOrderIds.All(excluded => o.IdOrder != excluded));
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