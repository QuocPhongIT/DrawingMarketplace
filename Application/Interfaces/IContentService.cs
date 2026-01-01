using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Application.DTOs.Pagination;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using static DrawingMarketplace.Application.DTOs.Content.ContentUpsertDto;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IContentService
    {
        Task<PagedResultDto<ContentListDto>> GetPagedPublicAsync(
            int page,
            int pageSize,
            string? keyword = null,
            string? categoryName = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            ContentSortBy sortBy = ContentSortBy.Newest,
            SortDirection sortDir = SortDirection.Desc);

        Task<ContentDetailDto?> GetPublicDetailAsync(Guid contentId);

        Task<PagedResultDto<ContentListDto>> GetPagedManagementAsync(
            int page,
            int pageSize,
            string? keyword = null,
            string? categoryName = null,
            ContentStatus? status = null,
            Guid? collaboratorId = null,
            ContentSortBy sortBy = ContentSortBy.Newest,
            SortDirection sortDir = SortDirection.Desc);

        Task<ContentDetailDto?> GetManagementDetailAsync(Guid contentId);

        Task<ContentDetailDto> CreateAsync(CreateContentDto dto);
        Task<ContentDetailDto?> UpdateAsync(Guid id, UpdateContentDto dto);
        Task<bool> DeleteAsync(Guid id);

        Task<ContentDetailDto?> ApproveContentAsync(Guid contentId, bool publish);

        Task<Content?> GetEntityByIdAsync(Guid contentId, CancellationToken ct = default);
    }
}