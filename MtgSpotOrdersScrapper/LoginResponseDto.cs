namespace MtgSpotOrdersScrapper;

// ReSharper disable once ClassNeverInstantiated.Global
public class LoginResponseDto
{
    public string? AccessToken { get; set; }
    public int ExpiresIn { get; set; }
    public string? Login { get; set; }
    public string? RefreshToken { get; set; }
    public string? TokenType { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}
