using System.ComponentModel.DataAnnotations;

namespace DrawingMarketplace.Application.DTOs.Auth
{
    public sealed record ResetPasswordRequest(
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        string Email,

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8 đến 100 ký tự.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.,;:#_+=-])[A-Za-z\d@$!%*?&.,;:#_+=-]{8,}$",
            ErrorMessage = "Mật khẩu mới phải chứa ít nhất: 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt (@$!%*?&.,;:#_+=-).")]
        string NewPassword
    );
}