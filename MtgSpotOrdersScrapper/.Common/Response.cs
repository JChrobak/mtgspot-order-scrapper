namespace MtgSpotOrdersScrapper.Common;

public class DataResponse<T>
{
    public T? Data { get; set; }
    public int Count { get; set; }
}

public class ItemResponse<T>
{
    public int ItemCount => Items?.Data?.Count ?? 0;
    public List<T>? ItemList => Items?.Data;
    public DataResponse<List<T>>? Items { get; set; }
    public int Count { get; set; }
}