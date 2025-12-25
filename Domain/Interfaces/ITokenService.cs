using DrawingMarketplace.Application.DTOs.Auth;
using DrawingMarketplace.Domain.Entities;

namespace DrawingMarketplace.Domain.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, IEnumerable<string> roles);

        Task<string> GenerateAndStoreRefreshTokenAsync(
            Guid userId,
            string? ipAddress = null,
            string? device = null
        );

        Task<AuthTokenDTO> RefreshAsync(
            string refreshToken,
            string? ipAddress = null,
            string? device = null
        );
        Task LogoutAsync(string refreshToken, string ip, string device);
        Task LogoutAllDevicesAsync(Guid userId);


        int GetAccessTokenExpiryInSeconds();
    }
}
