namespace DrawingMarketplace.Application.DTOs.Auth
{
    public sealed class ResetPasswordRequest
    {
        public string Email { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

}
