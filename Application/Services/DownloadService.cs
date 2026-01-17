using CloudinaryDotNet;
using DrawingMarketplace.Application.DTOs.Download;
using DrawingMarketplace.Application.DTOs.MediaFile;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
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
        private readonly Cloudinary _cloudinary;

        public DownloadService(
            DrawingMarketplaceContext context,
            ICurrentUserService currentUserService,
            Cloudinary cloudinary)
        {
            _context = context;
            _currentUserService = currentUserService;
            _cloudinary = cloudinary;
        }

        public async Task<List<MediaFileDto>> GetDownloadFilesAsync(Guid contentId)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("Người dùng chưa được xác thực.");

            var hasPermission = await _context.OrderItems
                .AnyAsync(oi => oi.ContentId == contentId && oi.Order!.UserId == userId);

            if (!hasPermission)
                throw new ForbiddenException("Bạn chưa mua nội dung này.");

            return await _context.Files
                .AsNoTracking()
                .Where(f => f.ContentId == contentId && f.Purpose == FilePurpose.downloadable)
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.CreatedAt)
                .Select(f => new MediaFileDto
                {
                    Id = f.Id,
                    PublicId = f.PublicId,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    Size = f.Size
                })
                .ToListAsync();
        }

        public async Task<DownloadFileResult> DownloadFileAsync(Guid contentId, Guid fileId)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("Người dùng chưa được xác thực.");

            var hasPurchased = await _context.OrderItems
                .AnyAsync(oi => oi.ContentId == contentId && oi.Order!.UserId == userId);

            if (!hasPurchased)
                throw new ForbiddenException("Bạn chưa mua nội dung này.");

            var file = await _context.Files
                .AsNoTracking()
                .FirstOrDefaultAsync(f =>
                    f.Id == fileId &&
                    f.ContentId == contentId &&
                    f.Purpose == FilePurpose.downloadable);

            if (file == null)
                throw new NotFoundException("MediaFile", fileId);

            // Get allowed downloads from all user's purchases for this content
            var allowedDownloads = await _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order!.UserId == userId && oi.ContentId == contentId)
                .SumAsync(oi => oi.Quantity * 5);

            // Get current download count
            var download = await _context.Downloads
                .FirstOrDefaultAsync(d => d.UserId == userId && d.ContentId == contentId);

            if (allowedDownloads == 0)
            {
                throw new ForbiddenException("Bạn chưa mua nội dung này.");
            }

            var currentDownloads = download?.DownloadCount ?? 0;
            if (currentDownloads >= allowedDownloads)
            {
                throw new ForbiddenException($"Bạn đã hết {allowedDownloads} lượt tải cho nội dung này.");
            }

            // Increment download count
            if (download == null)
            {
                _context.Downloads.Add(new Download
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ContentId = contentId,
                    DownloadCount = 1,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                download.DownloadCount++;
            }

            var signedUrl = _cloudinary.Api.Url
                .ResourceType("raw")
                .Type("private")
                .Signed(true)
                .BuildUrl(file.PublicId);

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(signedUrl);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Không thể tải file từ Cloudinary.");

            var stream = await response.Content.ReadAsStreamAsync();

            await _context.SaveChangesAsync();

            return new DownloadFileResult
            {
                Stream = stream,
                FileName = file.FileName,              // VD: drawing.pdf
                ContentType = file.FileType            // VD: application/pdf
            };
        }

    }
}
