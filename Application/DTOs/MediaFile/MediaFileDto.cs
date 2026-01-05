namespace DrawingMarketplace.Application.DTOs.MediaFile
{
    public class MediaFileDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = "";
        public string FileUrl { get; set; } = "";
        public string? FileType { get; set; }
        public long? Size { get; set; }
    }
}
