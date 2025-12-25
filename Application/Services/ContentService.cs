using AutoMapper;
using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Infrastructure.Persistence;
using static DrawingMarketplace.Application.DTOs.Content.ContentUpsertDto;

namespace DrawingMarketplace.Application.Services
{
    public class ContentService
        : CrudService<Content, ContentDto, CreateContentDto, UpdateContentDto>,
          IContentService
    {
        public ContentService(DrawingMarketplaceContext context, IMapper mapper)
            : base(context, mapper)
        {
        }
        public async Task<ContentDto?> ApproveContentAsync(Guid contentId, bool publish)
        {
            var content = await _dbSet.FindAsync(contentId);
            if (content == null) return null;

            content.Status = publish ? ContentStatus.published : ContentStatus.archived;
            content.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<ContentDto>(content);
        }

    }
}
