using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Infrastructure.Services;
using System.Security.Cryptography;
using DrawingMarketplace.Application.Interfaces;

namespace DrawingMarketplace.Application.Features.Auth
{
    public sealed class VerifyOtpHandler
    {
        private readonly IOtpRepository _otps;
        private readonly IUserRepository _users;
        private readonly IConfiguration _config;

        public VerifyOtpHandler(
            IOtpRepository otps,
            IUserRepository users,
            IConfiguration config)
        {
            _otps = otps;
            _users = users;
            _config = config;
        }

        public async Task ExecuteAsync(string email, string otp)
        {
            email = email.Trim().ToLower();

            var otpEntity = await _otps.GetLatestOtpAsync(email, OtpType.verify_account)
                ?? throw new NotFoundException("OTP", email);

            if (otpEntity.IsUsed)
                throw new ConflictException("OTP already used");

            if (otpEntity.ExpiredAt <= DateTime.UtcNow)
                throw new ConflictException("OTP expired");

            var secret = _config["Otp:Secret"]
                ?? throw new InvalidOperationException("Otp:Secret not configured");

            var hash = OtpHasher.Hash(otp, secret);

            if (!CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(hash),
                    Convert.FromHexString(otpEntity.CodeHash)))
                throw new BadRequestException("OTP invalid");

            otpEntity.IsUsed = true;
            await _otps.UpdateAsync(otpEntity);

            var user = await _users.GetByEmailAsync(email)
                ?? throw new NotFoundException("User", email);

            if (user.Status != UserStatus.active)
            {
                user.Status = UserStatus.active;
                await _users.UpdateAsync(user);
            }
        }
    }
}
