using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.Content
{
    public class ContentStatsAdminDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ThumbnailUrl { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public ContentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public Guid CollaboratorId { get; set; }

        public string CollaboratorUsername { get; set; } = string.Empty;

        public int Views { get; set; }

        public int Purchases { get; set; }

        public int Downloads { get; set; }

        public decimal TotalRevenue => Price * Purchases;      
        public double ConversionRate => Views > 0
            ? Math.Round((double)Purchases / Views * 100, 2)
            : 0;
    }
}
