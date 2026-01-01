using System.ComponentModel.DataAnnotations;

namespace DrawingMarketplace.Application.DTOs.Banner
{
    public class CreateBannerDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }

        [Required(ErrorMessage = "Ảnh banner là bắt buộc")]
        public IFormFile ImageFile { get; set; } = null!;

        public string? Button1Text { get; set; }
        public string? Button1Link { get; set; }
        public string? Button2Text { get; set; }
        public string? Button2Link { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}
