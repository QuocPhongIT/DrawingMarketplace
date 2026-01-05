namespace DrawingMarketplace.Application.DTOs.CopyrightReport
{
    public sealed class CreateCopyrightReportRequest
    {
        public Guid ContentId { get; set; }
        public string Reason { get; set; } = null!;
    }
}
