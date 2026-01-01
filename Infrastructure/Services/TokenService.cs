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
        var key = _config["Jwt:Key"];
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(key))
            throw new BadRequestException("Jwt__Key is missing in configuration");

        if (string.IsNullOrWhiteSpace(issuer))
            throw new BadRequestException("Jwt__Issuer is missing in configuration");

        if (string.IsNullOrWhiteSpace(audience))
            throw new BadRequestException("Jwt__Audience is missing in configuration");

        if (Encoding.UTF8.GetBytes(key).Length < 32)
            throw new BadRequestException("Jwt__Key must be at least 256 bits");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Iss, issuer),
            new(JwtRegisteredClaimNames.Aud, audience)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(
            _config.GetValue<int>("Jwt:AccessTokenMinutes", 60));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateAndStoreRefreshTokenAsync(
        Guid userId,
        string? ipAddress = null,
        string? device = null)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var days = _config.GetValue<int>("Jwt:RefreshTokenDays", 7);

        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = hash,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(days),
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
            .Include(x => x.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.TokenHash == hash)
            ?? throw new UnauthorizedException("Invalid refresh token");

        if (token.RevokedAt != null)
        {
            await RevokeAllUserTokensAsync(token.UserId!.Value);
            throw new ConflictException("Refresh token reuse detected");
        }

        if (token.ExpiredAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token expired");

        var user = token.User
            ?? throw new UnauthorizedException("User not found");

        var roles = user.UserRoles
            .Where(x => x.Role != null)
            .Select(x => x.Role!.Name)
            .ToList();

        if (!roles.Any())
            throw new ForbiddenException("User has no role assigned");

        var newRefreshToken =
            await GenerateAndStoreRefreshTokenAsync(user.Id, ipAddress, device);

        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByToken = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(newRefreshToken)));
        token.IpAddress = ipAddress;
        token.Device = device;

        await _db.SaveChangesAsync();

        return new AuthTokenDTO
        {
            AccessToken = GenerateAccessToken(user, roles),
            RefreshToken = newRefreshToken,
            ExpiresIn = GetAccessTokenExpiryInSeconds(),
            TokenType = "Bearer"
        };
    }

    public async Task LogoutAsync(string refreshToken, string ip, string device)
    {
        var hash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash);

        if (token == null) return;

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

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByToken = null;
        }

        await _db.SaveChangesAsync();
    }

    private async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public int GetAccessTokenExpiryInSeconds()
    {
        return _config.GetValue<int>("Jwt:AccessTokenMinutes", 60) * 60;
    }
}
