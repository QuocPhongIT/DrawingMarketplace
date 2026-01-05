namespace DrawingMarketplace.Application.DTOs.Reviews
{
    public class ReviewDto
    {
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }

        public Guid? ContentId { get; set; }

        public int? Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
