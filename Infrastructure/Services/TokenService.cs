using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DrawingMarketplace.Application.DTOs.Auth;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DrawingMarketplace.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly DrawingMarketplaceContext _db;

    public TokenService(IConfiguration config, DrawingMarketplaceContext db)
    {
        _config = config;
        _db = db;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(
            _config.GetValue<int>("Jwt:AccessTokenMinutes"));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateAndStoreRefreshTokenAsync(
        Guid userId,
        string? ipAddress = null,
        string? device = null)
    {
        var rawToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        var hash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var refreshTokenDays =
            _config.GetValue<int>("Jwt:RefreshTokenDays");

        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = hash,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(refreshTokenDays),
            IpAddress = ipAddress,
            Device = device
        });

        await _db.SaveChangesAsync();
        return rawToken;
    }

    public async Task<AuthTokenDTO> RefreshAsync(
        string refreshToken,
        string? ipAddress = null,
        string? device = null)
    {
        var hash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

        var token = await _db.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash)
            ?? throw new UnauthorizedException("Invalid refresh token");

        if (token.RevokedAt != null)
        {
            await RevokeAllUserTokensAsync(token.UserId!.Value);
            throw new UnauthorizedException("Refresh token reuse detected");
        }

        if (token.ExpiredAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token expired");

        var user = token.User
            ?? throw new UnauthorizedException("User not found");

        var newRefreshToken = await GenerateAndStoreRefreshTokenAsync(
            user.Id,
            ipAddress,
            device);

        var newHash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(newRefreshToken)));

        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByToken = newHash;
        token.IpAddress = ipAddress;
        token.Device = device;

        await _db.SaveChangesAsync();

        var roles = user.UserRoles
            .Select(x => x.Role.Name)
            .ToList();

        return new AuthTokenDTO
        {
            AccessToken = GenerateAccessToken(user, roles),
            RefreshToken = newRefreshToken,
            ExpiresIn = GetAccessTokenExpiryInSeconds(),
            TokenType = "Bearer"
        };
    }

    private async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }
    public async Task LogoutAsync(string refreshToken, string ip, string device)
    {
        var hash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash);

        if (token == null)
            return;

        token.RevokedAt = DateTime.UtcNow;
        token.IpAddress = ip;
        token.Device = device;

        await _db.SaveChangesAsync();
    }
    public async Task LogoutAllDevicesAsync(Guid userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync();

        if (!tokens.Any())
            return;

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByToken = null;
        }

        await _db.SaveChangesAsync();
    }
    public int GetAccessTokenExpiryInSeconds()
    {
        return _config.GetValue<int>("Jwt:AccessTokenMinutes") * 60;
    }
}
