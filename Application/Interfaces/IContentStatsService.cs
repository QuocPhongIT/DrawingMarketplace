using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Application.DTOs.Pagination;
using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IContentStatsService
    {
        Task<PagedResultDto<ContentStatsAdminDto>> GetAllStatsAsync(
        int page = 1,
        int pageSize = 20,
        string? keyword = null,
        Guid? collaboratorId = null,
        ContentSortBy sortBy = ContentSortBy.Sold,
        SortDirection sortDir = SortDirection.Desc);
        Task<PagedResultDto<ContentStatsCollaboratorDto>> GetMyStatsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20,
            string? keyword = null);
    }
}
