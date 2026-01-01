namespace DrawingMarketplace.Application.DTOs.Banner
{
    public class BannerDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Button1Text { get; set; }
        public string? Button1Link { get; set; }
        public string? Button2Text { get; set; }
        public string? Button2Link { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
