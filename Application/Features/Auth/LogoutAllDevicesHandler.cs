using DrawingMarketplace.Domain.Interfaces;

namespace DrawingMarketplace.Application.Features.Auth
{
    public sealed class LogoutAllDevicesHandler
    {
        private readonly ITokenService _tokens;

        public LogoutAllDevicesHandler(ITokenService tokens)
        {
            _tokens = tokens;
        }

        public async Task ExecuteAsync(Guid userId)
        {
            await _tokens.LogoutAllDevicesAsync(userId);
        }
    }
}
