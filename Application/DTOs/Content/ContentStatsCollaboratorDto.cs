using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.Content
{
    public class ContentStatsCollaboratorDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ThumbnailUrl { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public ContentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public int Views { get; set; }

        public int Purchases { get; set; }  

        public int Downloads { get; set; }     

        public decimal Earnings => Price * Purchases; 
    }
}
