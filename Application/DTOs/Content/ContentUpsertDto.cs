using System.ComponentModel.DataAnnotations;

namespace DrawingMarketplace.Application.DTOs.Content
{
    public class ContentUpsertDto
    {
        public class CreateContentDto
        {
            [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            
            [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
            public decimal Price { get; set; }
            public Guid? CategoryId { get; set; }
            public Guid? CollaboratorId { get; set; }
            
            [Required(ErrorMessage = "Ảnh thumbnail là bắt buộc")]
            public IFormFile ThumbnailFile { get; set; } = null!;
            
            public List<IFormFile>? PreviewFiles { get; set; }
            
            public List<IFormFile>? DownloadableFiles { get; set; }
        }

        public class UpdateContentDto
        {
            [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            
            [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
            public decimal Price { get; set; }
            public Guid? CategoryId { get; set; }
            
            public IFormFile? ThumbnailFile { get; set; }
            
            public List<IFormFile>? PreviewFiles { get; set; }
            
            public List<IFormFile>? DownloadableFiles { get; set; }
            
            public List<Guid>? FilesToDelete { get; set; }
        }
    }
}
