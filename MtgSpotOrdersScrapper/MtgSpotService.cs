using System.Net.Http.Headers;
using MtgSpotOrdersScrapper.Common;
using MtgSpotOrdersScrapper.Common.Extensions;
using MtgSpotOrdersScrapper.Contracts;

namespace MtgSpotOrdersScrapper;

public class MtgSpotService
{
    private readonly ConfigurationCredentials _credentials;
    private readonly ConfigurationHttpClientSettings _clientSettings;
    private readonly HttpClient _client;
    private LoginResponseDto? _loginData;

    public MtgSpotService(ConfigurationManager configurationManager)
    {
        _clientSettings = configurationManager.HttpClientSettings;
        _credentials = configurationManager.Credentials;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_clientSettings.BaseUrl);
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private FormUrlEncodedContent PrepareLoginRequestContent(string? username = null, string? password = null)
    {
        username ??= _credentials.Username;
        password ??= _credentials.Password;
        var pms = new Dictionary<string, string>
        {
            {"username", username},
            {"password", password},
            {"grant_type", "password"},
            {"client_id", _clientSettings.ClientId},
            {"keep_me_logged_in", "1"}
        };
        return new FormUrlEncodedContent(pms);
    }

    public async Task<IResult<Empty>> Login(string? username = null, string? password = null)
    {
        var loginResponse =
            await _client.PostAsync(_clientSettings.LoginUrl, PrepareLoginRequestContent(username, password));
        if (!loginResponse.IsSuccessStatusCode)
        {
            var content = await loginResponse.Content.ReadAsStringAsync();
            var errorMessage = $"Error logging in.\n" +
                               $"Response status: {loginResponse.StatusCode}\n" +
                               $"Response content: {content}";
            return Result<Empty>.Failure(errorMessage);
        }

        var loginData = await loginResponse.DeserializeContentAsync<LoginResponseDto>();
        if (loginData?.Error is not null)
        {
            var errorMessage = $"Error logging in.\n" +
                               $"Error: {loginData.Error}\n" +
                               $"Error message: {loginData.ErrorDescription}";
            return Result<Empty>.Failure(errorMessage);
        }

        _loginData = loginData;
        return Result<Empty>.Success(new Empty());
    }

    public async Task<IResult<List<ItemDto>>> GetOrderItems(OrderDto order)
    {
        if (_loginData is null)
        {
            return "User not logged in".Failure<List<ItemDto>>();
        }

        var response = await _client.GetAsync($"{order.OrderLink}?access_token={_loginData.AccessToken}");
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var errorMessage = $"Error fetching order items.\n" +
                               $"Response status: {response.StatusCode}\n" +
                               $"Response content: {content}";
            return errorMessage.Failure<List<ItemDto>>();
        }

        var itemsResponse = await response.DeserializeContentAsync<ItemResponse<ItemDto>>();
        var items = itemsResponse?.ItemList ?? new List<ItemDto>();
        return items.Success();
    }

    public async Task<IResult<List<OrderDto>>> GetOrders()
    {
        if (_loginData is null)
        {
            return "User not logged in".Failure<List<OrderDto>>();
        }

        var ordersResponse =
            await _client.GetAsync($"{_clientSettings.OrdersUrl}?access_token={_loginData.AccessToken}");
        if (!ordersResponse.IsSuccessStatusCode)
        {
            var content = await ordersResponse.Content.ReadAsStringAsync();
            var errorMessage = $"Error fetching orders.\n" +
                               $"Response status: {ordersResponse.StatusCode}\n" +
                               $"Response content: {content}";
            return errorMessage.Failure<List<OrderDto>>();
        }

        var orders = await ordersResponse.DeserializeContentAsync<DataResponse<List<OrderDto>>>();
        if (orders?.Data is null || orders.Count == 0)
            return "No orders found".Failure<List<OrderDto>>();
        return orders.Data.Success();
    }
}