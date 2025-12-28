using DrawingMarketplace.Application.DTOs.Auth;
using DrawingMarketplace.Application.Features.Auth;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;

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
    private readonly VerifyResetPasswordOtpHandler _verifyReset;
    private readonly ResetPasswordHandler _reset;
    private readonly ITokenService _tokens;

    public AuthController(
        RegisterHandler register,
        LoginHandler login,
        LogoutHandler logout,
        LogoutAllDevicesHandler logoutAll,
        VerifyOtpHandler verifyOtp,
        ResendOtpHandler resendOtp,
        ForgotPasswordHandler forgot,
        VerifyResetPasswordOtpHandler verifyReset,
        ResetPasswordHandler reset,
        ITokenService tokens)
    {
        _register = register;
        _login = login;
        _logout = logout;
        _logoutAll = logoutAll;
        _verifyOtp = verifyOtp;
        _resendOtp = resendOtp;
        _forgot = forgot;
        _verifyReset = verifyReset;
        _reset = reset;
        _tokens = tokens;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        await _register.ExecuteAsync(req);
        return Accepted();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthTokenDTO>> Login(LoginRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var device = Request.Headers["User-Agent"].ToString();

        var result = await _login.ExecuteAsync(
            req.Email, req.Password, ip, device);

        return Ok(result);
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
    public async Task<IActionResult> LogoutAll()
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _logoutAll.ExecuteAsync(userId);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokenDTO>> Refresh(RefreshTokenRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var device = Request.Headers["User-Agent"].ToString();

        var tokens = await _tokens.RefreshAsync(
            req.RefreshToken, ip, device);

        return Ok(tokens);
    }

    [AllowAnonymous]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest req)
    {
        await _verifyOtp.ExecuteAsync(req.Email, req.Otp);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(ResendOtpRequest req)
    {
        await _resendOtp.ExecuteAsync(req.Email);
        return Accepted();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
    {
        await _forgot.ExecuteAsync(req.Email);
        return Accepted();
    }

    [AllowAnonymous]
    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyResetOtp(VerifyResetOtpRequest req)
    {
        await _verifyReset.ExecuteAsync(req.Email, req.Otp);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
    {
        await _reset.ExecuteAsync(req.Email, req.NewPassword);
        return NoContent();
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        return Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.FindFirstValue(ClaimTypes.Email),
            role = User.FindFirstValue(ClaimTypes.Role)
        });
    }
}
