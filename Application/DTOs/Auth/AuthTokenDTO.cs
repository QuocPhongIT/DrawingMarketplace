namespace DrawingMarketplace.Application.DTOs.Auth
{
    public class AuthTokenDTO
    {
        public string AccessToken { get; init; } = null!;
        public string RefreshToken { get; init; } = null!;
        public int ExpiresIn { get; init; } 
        public string TokenType { get; init; } = "Bearer";
    }
}
