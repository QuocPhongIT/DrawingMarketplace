using System.ComponentModel.DataAnnotations;

namespace DrawingMarketplace.Application.DTOs.Auth
{
    public sealed record ResendOtpRequest(
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        string Email
    );
}