namespace DrawingMarketplace.Application.DTOs.Content
{
    public class ContentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? CollaboratorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ContentStatsDto? Stats { get; set; }
    }
}