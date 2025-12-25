using DrawingMarketplace.Application.DTOs.Auth;
using DrawingMarketplace.Domain.Interfaces;

namespace DrawingMarketplace.Application.Features.Auth
{
    public sealed class RefreshTokenHandler
    {
        private readonly ITokenService _tokens;

        public RefreshTokenHandler(ITokenService tokens)
        {
            _tokens = tokens;
        }

        public Task<AuthTokenDTO> HandleAsync(
            string refreshToken,
            string? ipAddress = null,
            string? device = null)
        {
            return _tokens.RefreshAsync(refreshToken, ipAddress, device);
        }
    }
}
