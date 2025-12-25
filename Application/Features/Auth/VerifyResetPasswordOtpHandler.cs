using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Infrastructure.Services;

namespace DrawingMarketplace.Application.Features.Auth
{
    public sealed class VerifyResetPasswordOtpHandler
    {
        private readonly IOtpRepository _otps;
        private readonly IConfiguration _config;

        public VerifyResetPasswordOtpHandler(
            IOtpRepository otps,
            IConfiguration config)
        {
            _otps = otps;
            _config = config;
        }

        public async Task ExecuteAsync(string email, string otp)
        {
            var otpEntity = await _otps.GetLatestOtpAsync(email, OtpType.reset_password)
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
        }
    }
}
