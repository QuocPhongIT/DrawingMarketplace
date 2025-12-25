using DrawingMarketplace.Domain.Interfaces;

namespace DrawingMarketplace.Application.Features.Auth
{
    public sealed class LogoutHandler
    {
        private readonly ITokenService _tokens;
        private readonly IHttpContextAccessor _http;

        public LogoutHandler(
            ITokenService tokens,
            IHttpContextAccessor http)
        {
            _tokens = tokens;
            _http = http;
        }

        public async Task ExecuteAsync(string refreshToken)
        {
            var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var device = _http.HttpContext?.Request.Headers["User-Agent"].ToString();

            await _tokens.LogoutAsync(
                refreshToken,
                ip ?? "unknown",
                device ?? "unknown");
        }
    }

}
