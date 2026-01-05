using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.CopyrightReport
{
    public sealed class CopyrightReportDto
    {
        public Guid Id { get; set; }

        public Guid? ContentId { get; set; }
        public string? ContentTitle { get; set; }

        public Guid? ReporterId { get; set; }
        public string? ReporterEmail { get; set; }

        public string? Reason { get; set; }
        public ReportStatus Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
