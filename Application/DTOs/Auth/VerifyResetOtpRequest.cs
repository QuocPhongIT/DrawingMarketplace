namespace DrawingMarketplace.Application.DTOs.Auth
{
    public sealed class VerifyResetOtpRequest
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
}
