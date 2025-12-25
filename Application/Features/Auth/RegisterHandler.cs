using DrawingMarketplace.Application.DTOs.Auth;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Domain.Exceptions;

namespace DrawingMarketplace.Application.Features.Auth
{
    public sealed class RegisterHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IOtpRepository _otpRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;

        private static readonly Guid USER_ROLE_ID =
            Guid.Parse("082173f4-60a1-473b-9bf0-ef6d81499e68");

        public RegisterHandler(
            IUserRepository userRepository,
            IOtpRepository otpRepository,
            IPasswordHasher passwordHasher,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
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

            var otpCode = Random.Shared.Next(100000, 999999).ToString();

            var otp = new Otp
            {
                Id = Guid.NewGuid(),
                Email = user.Email,
                Type = OtpType.verify_account,
                CodeHash = _passwordHasher.Hash(otpCode),
                ExpiredAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _otpRepository.AddAsync(otp);
            await _emailService.SendOtpAsync(user.Email, otpCode);
        }
    }
}
