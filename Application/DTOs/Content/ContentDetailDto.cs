using DrawingMarketplace.Application.DTOs.MediaFile;
using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.Content
{
    public class ContentDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public ContentStatus Status { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? CollaboratorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ContentStatsDto? Stats { get; set; }

        public string ThumbnailUrl { get; set; } = null!;

        public List<MediaFileDto> PreviewFiles { get; set; } = new();
        public List<MediaFileDto> DownloadableFiles { get; set; } = new();
    }
}
