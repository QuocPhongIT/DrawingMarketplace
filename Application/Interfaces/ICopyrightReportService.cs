using DrawingMarketplace.Application.DTOs.CopyrightReport;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface ICopyrightReportService
    {
        Task CreateAsync(CreateCopyrightReportRequest request);

        Task<List<CopyrightReportDto>> GetAllAsync();

        Task<CopyrightReportDto?> GetByIdAsync(Guid id);

        Task<bool> ApproveAsync(Guid reportId);

        Task<bool> RejectAsync(Guid reportId);
    }
}
