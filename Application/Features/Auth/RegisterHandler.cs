using DrawingMarketplace.Application.DTOs.Auth;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Application.Interfaces;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using DrawingMarketplace.Infrastructure.Services;

namespace DrawingMarketplace.Application.Features.Auth
{
    public sealed class RegisterHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IOtpRepository _otpRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        private static readonly Guid USER_ROLE_ID =
            Guid.Parse("082173f4-60a1-473b-9bf0-ef6d81499e68");

        public RegisterHandler(
            IUserRepository userRepository,
            IOtpRepository otpRepository,
            IPasswordHasher passwordHasher,
            IEmailService emailService,
            IConfiguration config)
        {
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _config = config;
        }

        public async Task ExecuteAsync(RegisterRequest request)
        {
            if (await _userRepository.ExistsByEmailAsync(request.Email))
                throw new ConflictException("Email already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Username = request.Username,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Status = UserStatus.inactive
            };

            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = USER_ROLE_ID
            });

            await _userRepository.AddAsync(user);

            await _otpRepository.InvalidateAsync(
                request.Email,
                OtpType.verify_account
            );

            var otpCode = RandomNumberGenerator
                .GetInt32(100000, 999999)
                .ToString();

            var secret = _config["Otp:Secret"];
            var expireMinutes = int.Parse(_config["Otp:ExpireMinutes"]);

            var otp = new Otp
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Type = OtpType.verify_account,
                CodeHash = OtpHasher.Hash(otpCode, secret),
                ExpiredAt = DateTime.UtcNow.AddMinutes(expireMinutes),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _otpRepository.AddAsync(otp);
            await _emailService.SendOtpAsync(request.Email, otpCode);
        }
    }
}
