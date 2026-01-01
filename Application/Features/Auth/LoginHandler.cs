using DrawingMarketplace.Application.DTOs.Auth;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Application.Interfaces;

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
            var user = await _users.GetByEmailAsync(email)
                ?? throw new UnauthorizedException("Invalid credentials");

            if (user.Status != UserStatus.active)
                throw new ForbiddenException("Account not verified");

            if (!_hasher.Verify(password, user.PasswordHash))
                throw new UnauthorizedException("Invalid credentials");

            var roles = user.UserRoles?
                .Where(x => x.Role != null)
                .Select(x => x.Role!.Name)
                .ToList()
                ?? new List<string>();

            if (!roles.Any())
                throw new ConflictException("User has no role assigned");

            return new AuthTokenDTO
            {
                AccessToken = _tokens.GenerateAccessToken(user, roles),
                RefreshToken = await _tokens.GenerateAndStoreRefreshTokenAsync(
                    user.Id, ipAddress, device),
                ExpiresIn = _tokens.GetAccessTokenExpiryInSeconds(),
                TokenType = "Bearer"
            };
        }
    }
}
