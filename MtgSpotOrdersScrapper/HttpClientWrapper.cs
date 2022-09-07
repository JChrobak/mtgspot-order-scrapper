using System.Net.Http.Headers;

namespace MtgSpotOrdersScrapper;

public class HttpClientWrapper
{
    private readonly ConfigurationCredentials _credentials;
    private readonly ConfigurationHttpClientSettings _clientSettings;
    private readonly HttpClient _client;
    private LoginResponseDto? _loginData;

    public HttpClientWrapper(ConfigurationManager configurationManager)
    {
        _clientSettings = configurationManager.HttpClientSettings;
        _credentials = configurationManager.Credentials;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_clientSettings.BaseUrl);
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private FormUrlEncodedContent PrepareLoginRequestContent()
    {
        var pms = new Dictionary<string, string>
        {
            {"username", _credentials.Username},
            {"password", _credentials.Password},
            {"grant_type", "password"},
            {"client_id", _clientSettings.ClientId},
            {"keep_me_logged_in", "1"}
        };
        return new FormUrlEncodedContent(pms);
    }

    public async Task<bool> Login()
    {
        var loginResponse = await _client.PostAsync(_clientSettings.LoginUrl, PrepareLoginRequestContent());
        if (!loginResponse.IsSuccessStatusCode)
        {
            var content = await loginResponse.Content.ReadAsStringAsync();
            await Console.Error.WriteLineAsync($"Error logging in.\n" +
                                               $"Response status: {loginResponse.StatusCode}\n" +
                                               $"Response content: {content}");
            return false;
        }
        
        var loginData = await loginResponse.DeserializeContentAsync<LoginResponseDto>();
        if (loginData?.Error is not null)
        {
            var content = await loginResponse.Content.ReadAsStringAsync();
            await Console.Error.WriteLineAsync($"Error logging in.\n" +
                                               $"Error: {loginData.Error}\n" +
                                               $"Error message: {loginData.ErrorDescription}");
            return false;
        }
        _loginData = loginData;
        await Console.Out.WriteLineAsync("Successfully logged in!");
        return true;
    }

    public async Task<List<ItemDto>> GetOrderItems(OrderDto order)
    {
        if (_loginData is null)
        {
            await Console.Error.WriteLineAsync("User not logged in");
            return new List<ItemDto>();
        }

        var itemsResponse = await _client.GetAsync($"{order.OrderLink}?access_token={_loginData.AccessToken}");
        if (!itemsResponse.IsSuccessStatusCode)
        {
            var content = await itemsResponse.Content.ReadAsStringAsync();
            await Console.Error.WriteLineAsync($"Error fetching order items.\n" +
                                               $"Response status: {itemsResponse.StatusCode}\n" +
                                               $"Response content: {content}");
            return new List<ItemDto>();
        }
        
        var items = await itemsResponse.DeserializeContentAsync<ItemResponse<ItemDto>>();
        await Console.Out.WriteLineAsync($"Successfully fetched {items?.ItemCount ?? 0} items " +
                                         $"for order {order.IdOrder}.");
        return items?.ItemList ?? new List<ItemDto>();
    }

    public async Task<List<OrderDto>> GetOrders()
    {
        if (_loginData is null)
        {
            await Console.Error.WriteLineAsync("User not logged in");
            return new List<OrderDto>();
        }

        var ordersResponse =
            await _client.GetAsync($"{_clientSettings.OrdersUrl}?access_token={_loginData.AccessToken}");
        if (!ordersResponse.IsSuccessStatusCode)
        {
            var content = await ordersResponse.Content.ReadAsStringAsync();
            await Console.Error.WriteLineAsync($"Error fetching orders.\n" +
                                               $"Response status: {ordersResponse.StatusCode}\n" +
                                               $"Response content: {content}");
            return new List<OrderDto>();
        }
        
        var orders = await ordersResponse.DeserializeContentAsync<DataResponse<List<OrderDto>>>();
        await Console.Out.WriteLineAsync($"Successfully fetched {orders?.Count ?? 0} orders.");
        return orders?.Data ?? new List<OrderDto>();
    }
}