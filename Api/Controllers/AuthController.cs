using System.Security.Claims;
using DrawingMarketplace.Application.DTOs.Auth;
using DrawingMarketplace.Application.Features.Auth;
using DrawingMarketplace.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrawingMarketplace.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterHandler _register;
    private readonly VerifyOtpHandler _verify;
    private readonly ResendOtpHandler _resend;
    private readonly LoginHandler _login;
    private readonly ForgotPasswordHandler _forgotPassword;
    private readonly ResetPasswordHandler _resetPassword;
    private readonly VerifyResetPasswordOtpHandler _verifyResetOtp;
    private readonly ITokenService _tokens;
    private readonly LogoutHandler _logout;
    private readonly LogoutAllDevicesHandler _logoutAllDevices;

    public AuthController(
        RegisterHandler register,
        VerifyOtpHandler verify,
        ResendOtpHandler resend,
        LoginHandler login,
        ForgotPasswordHandler forgotPassword,
        ResetPasswordHandler resetPassword,
        VerifyResetPasswordOtpHandler verifyResetOtp,
        ITokenService tokens,
        LogoutHandler logout,
        LogoutAllDevicesHandler logoutAllDevices)
    {
        _register = register;
        _verify = verify;
        _resend = resend;
        _login = login;
        _forgotPassword = forgotPassword;
        _resetPassword = resetPassword;
        _verifyResetOtp = verifyResetOtp;
        _tokens = tokens;
        _logout = logout;
        _logoutAllDevices = logoutAllDevices;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        await _register.ExecuteAsync(request);
        return Accepted();
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest req)
    {
        await _verify.ExecuteAsync(req.Email, req.Otp);
        return NoContent();
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(ResendOtpRequest req)
    {
        await _resend.ExecuteAsync(req.Email);
        return Accepted();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthTokenDTO>> Login(LoginRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var device = Request.Headers["User-Agent"].ToString();

        var data = await _login.ExecuteAsync(
            req.Email,
            req.Password,
            ip,
            device);

        return Ok(data);
    }


    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
    {
        await _forgotPassword.ExecuteAsync(req.Email);
        return Accepted();
    }

    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyResetOtp(VerifyResetOtpRequest req)
    {
        await _verifyResetOtp.ExecuteAsync(req.Email, req.Otp);
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
    {
        await _resetPassword.ExecuteAsync(req.Email, req.NewPassword);
        return NoContent();
    }


    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokenDTO>> Refresh(RefreshTokenRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var device = Request.Headers["User-Agent"].ToString();

        var data = await _tokens.RefreshAsync(
            req.RefreshToken,
            ip,
            device);

        return Ok(data);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest req)
    {
        await _logout.ExecuteAsync(req.RefreshToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAllDevices()
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _logoutAllDevices.ExecuteAsync(userId);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            IsAuth = User.Identity?.IsAuthenticated,
            Claims = User.Claims.Select(c => new
            {
                c.Type,
                c.Value
            })
        });
    }
}
