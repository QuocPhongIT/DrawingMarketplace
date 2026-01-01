using System.ComponentModel.DataAnnotations;

namespace DrawingMarketplace.Application.DTOs.Banner
{
    public class UpdateBannerDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }

        public IFormFile? ImageFile { get; set; } 

        public string? Button1Text { get; set; }
        public string? Button1Link { get; set; }
        public string? Button2Text { get; set; }
        public string? Button2Link { get; set; }

        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
