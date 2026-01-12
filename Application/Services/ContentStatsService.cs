using AutoMapper;
using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Application.DTOs.Pagination;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Infrastructure.Repositories;

namespace DrawingMarketplace.Application.Services
{
    public class ContentStatsService : IContentStatsService
    {
        private readonly IContentService _contentService;
        private readonly ICollaboratorRepository _collaboratorRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public ContentStatsService(
            IContentService contentService,
            ICollaboratorRepository collaboratorRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _contentService = contentService;
            _collaboratorRepository = collaboratorRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<ContentStatsAdminDto>> GetAllStatsAsync(
            int page = 1,
            int pageSize = 20,
            string? keyword = null,
            Guid? collaboratorId = null,
            ContentSortBy sortBy = ContentSortBy.Sold,
            SortDirection sortDir = SortDirection.Desc)
        {
            var result = await _contentService.GetPagedManagementAsync(
                page: page,
                pageSize: pageSize,
                keyword: keyword,
                status: null,
                collaboratorId: collaboratorId,
                sortBy: sortBy,
                sortDir: sortDir);

            var mappedItems = _mapper.Map<List<ContentStatsAdminDto>>(result.Items);

            return new PagedResultDto<ContentStatsAdminDto>
            {
                Items = mappedItems,
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultDto<ContentStatsCollaboratorDto>> GetMyStatsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20,
            string? keyword = null)
        {
            var collaborator = await _collaboratorRepository.GetByUserIdAsync(userId)
                ?? throw new ForbiddenException("Bạn không phải là collaborator");

            var result = await _contentService.GetPagedManagementAsync(
                page: page,
                pageSize: pageSize,
                keyword: keyword,
                status: null,
                collaboratorId: collaborator.Id);

            var mappedItems = _mapper.Map<List<ContentStatsCollaboratorDto>>(result.Items);

            return new PagedResultDto<ContentStatsCollaboratorDto>
            {
                Items = mappedItems,
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}