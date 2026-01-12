using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Api.Responses;
using DrawingMarketplace.Application.DTOs.Auth;
using DrawingMarketplace.Application.Features.Auth;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly RegisterHandler _register;
        private readonly LoginHandler _login;
        private readonly LogoutHandler _logout;
        private readonly LogoutAllDevicesHandler _logoutAll;
        private readonly VerifyOtpHandler _verifyOtp;
        private readonly ResendOtpHandler _resendOtp;
        private readonly ForgotPasswordHandler _forgot;
        private readonly ResetPasswordHandler _reset;
        private readonly ITokenService _tokens;
        private readonly DrawingMarketplaceContext _context;

        public AuthController(
            RegisterHandler register,
            LoginHandler login,
            LogoutHandler logout,
            LogoutAllDevicesHandler logoutAll,
            VerifyOtpHandler verifyOtp,
            ResendOtpHandler resendOtp,
            ForgotPasswordHandler forgot,
            ResetPasswordHandler reset,
            ITokenService tokens,
            DrawingMarketplaceContext context)
        {
            _register = register;
            _login = login;
            _logout = logout;
            _logoutAll = logoutAll;
            _verifyOtp = verifyOtp;
            _resendOtp = resendOtp;
            _forgot = forgot;
            _reset = reset;
            _tokens = tokens;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest req)
        {
            await _register.ExecuteAsync(req);
            return this.Success<object>(null, "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản", "Registration successful. Please check your email to verify your account", 202);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var device = Request.Headers["User-Agent"].ToString();

            var result = await _login.ExecuteAsync(
                req.Email, req.Password, ip, device);

            return this.Success(result, "Đăng nhập thành công", "Login successfully");
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenRequest req)
        {
            await _logout.ExecuteAsync(req.RefreshToken);
            return this.Success<object>(null, "Đăng xuất thành công", "Logout successfully");
        }

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _logoutAll.ExecuteAsync(userId);
            return this.Success<object>(null, "Đăng xuất tất cả thiết bị thành công", "Logout all devices successfully");
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequest req)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var device = Request.Headers["User-Agent"].ToString();

            var tokens = await _tokens.RefreshAsync(
                req.RefreshToken, ip, device);

            return this.Success(tokens, "Làm mới token thành công", "Refresh token successfully");
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequest req)
        {
            await _verifyOtp.ExecuteAsync(req.Email, req.Otp);
            return this.Success<object>(null, "Xác thực OTP thành công", "Verify OTP successfully");
        }

        [AllowAnonymous]
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp(ResendOtpRequest req)
        {
            await _resendOtp.ExecuteAsync(req.Email);
            return this.Success<object>(null, "Gửi lại OTP thành công", "Resend OTP successfully");
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
        {
            await _forgot.ExecuteAsync(req.Email);
            return this.Success<object>(null, "Đã gửi OTP đặt lại mật khẩu. Vui lòng kiểm tra email", "Password reset OTP sent. Please check your email");
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
        {
            await _reset.ExecuteAsync(
                req.Email,
                req.Otp,
                req.NewPassword);

            return this.Success<object>(
                null,
                "Đặt lại mật khẩu thành công",
                "Reset password successfully");
        }
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Token không hợp lệ");
            }

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Collaborator) 
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng");
            }
            string displayRole = user.Collaborator != null
                ? "Cộng tác viên"
                : user.UserRoles
                    .Select(ur => ur.Role?.Name ?? "Unknown")
                    .FirstOrDefault() ?? "Người dùng";

            var profile = new
            {
                id = user.Id,
                username = user.Username,               
                email = user.Email,                      
                status = user.Status.ToString(),
                createdAt = user.CreatedAt?.ToString("dd/MM/yyyy HH:mm"),
                updatedAt = user.UpdatedAt?.ToString("dd/MM/yyyy HH:mm"), 
                roleDisplay = displayRole,    

                isCollaborator = user.Collaborator != null,
                collaboratorStatus = user.Collaborator?.Status.ToString() 
            };

            return this.Success(profile, "Lấy thông tin profile thành công", "Get profile successfully");
        }
    }
}