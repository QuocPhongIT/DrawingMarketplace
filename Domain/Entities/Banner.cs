namespace DrawingMarketplace.Domain.Entities
{
    public class Banner
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        public string? Button1Text { get; set; }
        public string? Button1Link { get; set; }
        public string? Button2Text { get; set; }
        public string? Button2Link { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        public Guid? CreatedBy { get; set; }
        public User? CreatedByNavigation { get; set; }

        public Guid? UpdatedBy { get; set; }
        public User? UpdatedByNavigation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
