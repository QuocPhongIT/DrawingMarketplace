using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.Content
{
    public class ContentListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public ContentStatus Status { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string ThumbnailUrl { get; set; } = null!;
    }
}
