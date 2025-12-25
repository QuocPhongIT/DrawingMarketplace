using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Infrastructure.Services;
using System.Security.Cryptography;

namespace DrawingMarketplace.Application.Features.Auth
{
    public sealed class ForgotPasswordHandler
    {
        private readonly IUserRepository _users;
        private readonly IOtpRepository _otps;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;

        public ForgotPasswordHandler(
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

            if (user.Status != UserStatus.active)
                throw new ConflictException("Account not active");

            await _otps.InvalidateAsync(email, OtpType.reset_password);

            var otpCode = RandomNumberGenerator
                .GetInt32(100000, 999999)
                .ToString();

            var secret = _config["Otp:Secret"];
            var expireMinutes = int.Parse(_config["Otp:ExpireMinutes"]);

            var otp = new Otp
            {
                Id = Guid.NewGuid(),
                Email = email,
                Type = OtpType.reset_password,
                CodeHash = OtpHasher.Hash(otpCode, secret),
                ExpiredAt = DateTime.UtcNow.AddMinutes(expireMinutes),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            await _otps.AddAsync(otp);
            await _email.SendOtpAsync(email, otpCode);
        }
    }
}
