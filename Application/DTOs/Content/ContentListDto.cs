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

        public DateTime CreatedAt { get; set; }

        public string? ThumbnailUrl { get; set; }

        public List<string> PreviewUrls { get; set; } = new();

        public Guid CollaboratorId { get; set; } = Guid.Empty;

        public string CollaboratorUsername { get; set; } = string.Empty;

        public int Views { get; set; }

        public int Purchases { get; set; }

        public int Downloads { get; set; }

        public int Quantity { get; set; } = 1;
    }
}