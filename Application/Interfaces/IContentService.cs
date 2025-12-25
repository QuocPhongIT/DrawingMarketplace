using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Domain.Entities;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IContentService
        : ICrudService<Content, ContentDto, ContentUpsertDto.CreateContentDto, ContentUpsertDto.UpdateContentDto>
    {
        Task<ContentDto?> ApproveContentAsync(Guid contentId, bool publish);
    }
}
