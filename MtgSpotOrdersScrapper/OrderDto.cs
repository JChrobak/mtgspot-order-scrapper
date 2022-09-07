namespace MtgSpotOrdersScrapper;

public class OrderDto
{
    public string? AddDate { get; set; }
    public int? IdOrder { get; set; }
    public string? StatusName { get; set; }
    public string? ShippingCost { get; set; }
    public string? Total { get; set; }
    public List<LinkDto>? Links { get; set; }
    public string? OrderLink => Links?.First().Href;
}

public class LinkDto
{
    public string? Rel { get; set; }
    public string? Href { get; set; }
}