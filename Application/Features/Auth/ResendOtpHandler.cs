using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Infrastructure.Services;
using System.Security.Cryptography;
using DrawingMarketplace.Application.Interfaces;

namespace DrawingMarketplace.Application.Features.Auth
{
    public sealed class ResendOtpHandler
    {
        private readonly IUserRepository _users;
        private readonly IOtpRepository _otps;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;

        public ResendOtpHandler(
            IUserRepository users,
            IOtpRepository otps,
            IEmailService email,
            IConfiguration config)
        {
            _users = users;
            _otps = otps;
            _email = email;
            _config = config;
        }

        public async Task ExecuteAsync(string email)
        {
            var user = await _users.GetByEmailAsync(email)
                ?? throw new NotFoundException("User", email);

            if (user.Status == UserStatus.active)
                throw new ConflictException("Account already verified");

            await _otps.InvalidateAsync(email, OtpType.verify_account);

            var otpCode = RandomNumberGenerator
                .GetInt32(100000, 999999)
                .ToString();

            var secret = _config["Otp:Secret"];
            var expireMinutes = int.Parse(_config["Otp:ExpireMinutes"]);

            var otp = new Otp
            {
                Id = Guid.NewGuid(),
                Email = email,
                Type = OtpType.verify_account,
                CodeHash = OtpHasher.Hash(otpCode, secret),
                ExpiredAt = DateTime.UtcNow.AddMinutes(expireMinutes),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _otps.AddAsync(otp);
            await _email.SendOtpAsync(email, otpCode);
        }
    }
}
