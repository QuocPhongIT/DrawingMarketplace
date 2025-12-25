using DrawingMarketplace.Application.DTOs.Auth;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Domain.Exceptions;

namespace DrawingMarketplace.Application.Features.Auth
{
    public sealed class LoginHandler
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;

        public LoginHandler(
            IUserRepository users,
            IPasswordHasher hasher,
            ITokenService tokens)
        {
            _users = users;
            _hasher = hasher;
            _tokens = tokens;
        }

        public async Task<AuthTokenDTO> ExecuteAsync(
            string email,
            string password,
            string? ipAddress = null,
            string? device = null)
        {
            var user = await _users.GetByEmailAsync(email);
            if (user == null)
                throw new UnauthorizedException("Invalid credentials");

            if (user.Status != UserStatus.active)
                throw new UnauthorizedException("Account not verified");

            if (!_hasher.Verify(password, user.PasswordHash))
                throw new UnauthorizedException("Invalid credentials");

            var roles = user.UserRoles
                .Select(x => x.Role.Name)
                .ToList();

            var accessToken = _tokens.GenerateAccessToken(user, roles);

            return new AuthTokenDTO
            {
                AccessToken = accessToken,
                RefreshToken = await _tokens.GenerateAndStoreRefreshTokenAsync(
                    user.Id, ipAddress, device),
                ExpiresIn = _tokens.GetAccessTokenExpiryInSeconds(),
                TokenType = "Bearer"
            };
        }
    }
}
