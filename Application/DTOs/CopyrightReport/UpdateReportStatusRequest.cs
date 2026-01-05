using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.CopyrightReport
{
    public sealed class UpdateReportStatusRequest
    {
        public ReportStatus Status { get; set; }
    }
}
