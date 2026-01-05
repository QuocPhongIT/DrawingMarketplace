using DrawingMarketplace.Application.DTOs.MediaFile;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Application.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly DrawingMarketplaceContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DownloadService(
            DrawingMarketplaceContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<List<MediaFileDto>> GetDownloadFilesAsync(Guid contentId)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException();

            var hasPermission = await _context.Downloads.AnyAsync(d =>
                d.UserId == userId && d.ContentId == contentId);

            if (!hasPermission)
                throw new ForbiddenException("Bạn chưa mua nội dung này");

            return await _context.Files
                .Where(f =>
                    f.ContentId == contentId &&
                    f.Purpose == FilePurpose.downloadable)
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new MediaFileDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileUrl = f.FileUrl,
                    FileType = f.FileType,
                    Size = f.Size
                })
                .ToListAsync();
        }

    }
}
