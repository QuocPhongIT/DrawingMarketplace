using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace DrawingMarketplace.Application.Features.Auth;

public sealed class ResetPasswordHandler
{
    private readonly IUserRepository _users;
    private readonly IOtpRepository _otps;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IConfiguration _config;

    public ResetPasswordHandler(
        IUserRepository users,
        IOtpRepository otps,
        IPasswordHasher hasher,
        ITokenService tokens,
        IConfiguration config)
    {
        _users = users;
        _otps = otps;
        _hasher = hasher;
        _tokens = tokens;
        _config = config;
    }

    public async Task ExecuteAsync(
     string email,
     string otp,
     string newPassword)
    {
        var otpEntity = await _otps.GetLatestOtpAsync(
            email, OtpType.reset_password)
            ?? throw new NotFoundException("OTP", email);

        if (otpEntity.IsUsed)
            throw new ConflictException("OTP already used");

        if (otpEntity.ExpiredAt < DateTime.UtcNow)
            throw new ConflictException("OTP expired");

        var secret = _config["Otp:Secret"];
        var hash = OtpHasher.Hash(otp, secret);

        if (hash != otpEntity.CodeHash)
            throw new BadRequestException("OTP invalid");

        otpEntity.IsUsed = true;
        await _otps.UpdateAsync(otpEntity);
        var user = await _users.GetByEmailAsync(email)
            ?? throw new NotFoundException("User", email);

        if (_hasher.Verify(newPassword, user.PasswordHash))
            throw new ConflictException("New password must be different from the old password");
        user.PasswordHash = _hasher.Hash(newPassword);

        await _tokens.LogoutAllDevicesAsync(user.Id);

        await _users.UpdateAsync(user);
    }

}
