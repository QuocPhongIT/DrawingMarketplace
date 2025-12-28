using System.ComponentModel.DataAnnotations;

namespace DrawingMarketplace.Application.DTOs.Auth
{
    public sealed record RegisterRequest(
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        string Email,

        [Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên người dùng phải từ 3 đến 50 ký tự.")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$",
            ErrorMessage = "Tên người dùng chỉ được chứa chữ cái, số, dấu gạch dưới (_) và dấu gạch ngang (-).")]
        string Username,

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 100 ký tự.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.,;:#_+=-])[A-Za-z\d@$!%*?&.,;:#_+=-]{8,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất: 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt (@$!%*?&.,;:#_+=-).")]
        string Password
    );
}