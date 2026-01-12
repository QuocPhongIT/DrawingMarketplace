namespace DrawingMarketplace.Application.DTOs.Download
{
    public class DownloadFileResult
    {
        public Stream Stream { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string FileName { get; set; } = null!;
    }
}
