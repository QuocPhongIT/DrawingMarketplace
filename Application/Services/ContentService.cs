using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Application.DTOs.Pagination;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using static DrawingMarketplace.Application.DTOs.Content.ContentUpsertDto;

namespace DrawingMarketplace.Application.Services
{
    public class ContentService : IContentService
    {
        private readonly DrawingMarketplaceContext _context;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;

        public ContentService(DrawingMarketplaceContext context, IMapper mapper, Cloudinary cloudinary)
        {
            _context = context;
            _mapper = mapper;
            _cloudinary = cloudinary;
        }

        public async Task<PagedResultDto<ContentListDto>> GetPagedPublicAsync(
            int page,
            int pageSize,
            string? keyword = null,
            string? categoryName = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            ContentSortBy sortBy = ContentSortBy.Newest,
            SortDirection sortDir = SortDirection.Desc)
        {
            var query = _context.Contents
                .AsNoTracking()
                .Where(c => c.Status == ContentStatus.published);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{keyword}%"));

            if (!string.IsNullOrWhiteSpace(categoryName))
                query = query.Where(x => x.Category != null && EF.Functions.ILike(x.Category.Name, $"%{categoryName}%"));

            if (minPrice.HasValue) query = query.Where(x => x.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(x => x.Price <= maxPrice.Value);

            query = sortBy switch
            {
                ContentSortBy.Price => sortDir == SortDirection.Asc
                    ? query.OrderBy(x => x.Price)
                    : query.OrderByDescending(x => x.Price),
                ContentSortBy.Title => sortDir == SortDirection.Asc
                    ? query.OrderBy(x => x.Title)
                    : query.OrderByDescending(x => x.Title),
                ContentSortBy.Sold => query.OrderByDescending(x =>
                    x.ContentStat != null ? x.ContentStat.Purchases : 0),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(c => c.Files)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ContentListDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    Status = c.Status,
                    CategoryId = c.CategoryId,
                    CreatedAt = c.CreatedAt,
                    ThumbnailUrl = c.Files
                        .Where(f => f.Purpose == FilePurpose.thumbnail)
                        .Select(f => f.FileUrl)
                        .FirstOrDefault()
                        ?? "https://res.cloudinary.com/drawingmarketplace/image/upload/v1/default-thumbnail.jpg"
                })
                .ToListAsync();

            return new PagedResultDto<ContentListDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ContentDetailDto?> GetPublicDetailAsync(Guid contentId)
        {
            return await _context.Contents
                .AsNoTracking()
                .Include(c => c.Files)
                .Include(c => c.ContentStat)
                .Where(c => c.Id == contentId && c.Status == ContentStatus.published)
                .Select(c => new ContentDetailDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    Status = c.Status,
                    CategoryId = c.CategoryId,
                    CollaboratorId = c.CollaboratorId,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Stats = c.ContentStat != null
                        ? new ContentStatsDto
                        {
                            Views = c.ContentStat.Views ?? 0,
                            Purchases = c.ContentStat.Purchases ?? 0,
                            Downloads = c.ContentStat.Downloads ?? 0
                        }
                        : null,
                    ThumbnailUrl = c.Files
                        .Where(f => f.Purpose == FilePurpose.thumbnail)
                        .Select(f => f.FileUrl)
                        .FirstOrDefault()
                        ?? "https://res.cloudinary.com/drawingmarketplace/image/upload/v1/default-thumbnail.jpg",
                    PreviewUrls = c.Files
                        .Where(f => f.Purpose == FilePurpose.preview)
                        .OrderBy(f => f.DisplayOrder)
                        .ThenBy(f => f.CreatedAt)
                        .Select(f => f.FileUrl)
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResultDto<ContentListDto>> GetPagedManagementAsync(
            int page,
            int pageSize,
            string? keyword = null,
            string? categoryName = null,
            ContentStatus? status = null,
            Guid? collaboratorId = null,
            ContentSortBy sortBy = ContentSortBy.Newest,
            SortDirection sortDir = SortDirection.Desc)
        {
            var query = _context.Contents
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{keyword}%"));

            if (!string.IsNullOrWhiteSpace(categoryName))
                query = query.Where(x => x.Category != null && EF.Functions.ILike(x.Category.Name, $"%{categoryName}%"));

            if (status.HasValue) query = query.Where(x => x.Status == status.Value);
            if (collaboratorId.HasValue) query = query.Where(x => x.CollaboratorId == collaboratorId.Value);

            query = sortBy switch
            {
                ContentSortBy.Price => sortDir == SortDirection.Asc
                    ? query.OrderBy(x => x.Price)
                    : query.OrderByDescending(x => x.Price),
                ContentSortBy.Title => sortDir == SortDirection.Asc
                    ? query.OrderBy(x => x.Title)
                    : query.OrderByDescending(x => x.Title),
                ContentSortBy.Sold => query.OrderByDescending(x =>
                    x.ContentStat != null ? x.ContentStat.Purchases : 0),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(c => c.Files)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ContentListDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    Status = c.Status,
                    CategoryId = c.CategoryId,
                    CreatedAt = c.CreatedAt,
                    ThumbnailUrl = c.Files
                        .Where(f => f.Purpose == FilePurpose.thumbnail)
                        .Select(f => f.FileUrl)
                        .FirstOrDefault()
                        ?? "https://res.cloudinary.com/drawingmarketplace/image/upload/v1/default-thumbnail.jpg"
                })
                .ToListAsync();

            return new PagedResultDto<ContentListDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ContentDetailDto?> GetManagementDetailAsync(Guid contentId)
        {
            return await _context.Contents
                .AsNoTracking()
                .Include(c => c.Files)
                .Include(c => c.ContentStat)
                .Where(c => c.Id == contentId)
                .Select(c => new ContentDetailDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    Status = c.Status,
                    CategoryId = c.CategoryId,
                    CollaboratorId = c.CollaboratorId,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Stats = c.ContentStat != null
                        ? new ContentStatsDto
                        {
                            Views = c.ContentStat.Views ?? 0,
                            Purchases = c.ContentStat.Purchases ?? 0,
                            Downloads = c.ContentStat.Downloads ?? 0
                        }
                        : null,
                    ThumbnailUrl = c.Files
                        .Where(f => f.Purpose == FilePurpose.thumbnail)
                        .Select(f => f.FileUrl)
                        .FirstOrDefault()
                        ?? "https://res.cloudinary.com/drawingmarketplace/image/upload/v1/default-thumbnail.jpg",
                    PreviewUrls = c.Files
                        .Where(f => f.Purpose == FilePurpose.preview)
                        .OrderBy(f => f.DisplayOrder)
                        .ThenBy(f => f.CreatedAt)
                        .Select(f => f.FileUrl)
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ContentDetailDto> CreateAsync(CreateContentDto dto)
        {
            var content = _mapper.Map<Content>(dto);
            content.Status = ContentStatus.draft;
            content.CreatedAt = DateTime.UtcNow;
            content.UpdatedAt = DateTime.UtcNow;

            _context.Contents.Add(content);
            await _context.SaveChangesAsync();

            // Upload thumbnail
            if (dto.ThumbnailFile != null)
            {
                var thumbnailUrl = await UploadImageAsync(dto.ThumbnailFile, "drawing-marketplace/content/thumbnails");
                var thumbnailFile = new MediaFile
                {
                    Id = Guid.NewGuid(),
                    ContentId = content.Id,
                    FileName = dto.ThumbnailFile.FileName,
                    FileUrl = thumbnailUrl,
                    FileType = dto.ThumbnailFile.ContentType,
                    Size = dto.ThumbnailFile.Length,
                    Purpose = FilePurpose.thumbnail,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Files.Add(thumbnailFile);
            }

            // Upload preview images
            if (dto.PreviewFiles != null && dto.PreviewFiles.Any())
            {
                int order = 0;
                foreach (var previewFile in dto.PreviewFiles)
                {
                    var previewUrl = await UploadImageAsync(previewFile, "drawing-marketplace/content/previews");
                    var mediaFile = new MediaFile
                    {
                        Id = Guid.NewGuid(),
                        ContentId = content.Id,
                        FileName = previewFile.FileName,
                        FileUrl = previewUrl,
                        FileType = previewFile.ContentType,
                        Size = previewFile.Length,
                        Purpose = FilePurpose.preview,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Files.Add(mediaFile);
                }
            }

            // Upload downloadable files
            if (dto.DownloadableFiles != null && dto.DownloadableFiles.Any())
            {
                int order = 0;
                foreach (var downloadFile in dto.DownloadableFiles)
                {
                    var downloadUrl = await UploadFileAsync(downloadFile, "drawing-marketplace/content/downloads");
                    var mediaFile = new MediaFile
                    {
                        Id = Guid.NewGuid(),
                        ContentId = content.Id,
                        FileName = downloadFile.FileName,
                        FileUrl = downloadUrl,
                        FileType = downloadFile.ContentType,
                        Size = downloadFile.Length,
                        Purpose = FilePurpose.downloadable,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Files.Add(mediaFile);
                }
            }

            await _context.SaveChangesAsync();

            return await GetManagementDetailAsync(content.Id) ?? throw new Exception("Create failed");
        }

        public async Task<ContentDetailDto?> UpdateAsync(Guid id, UpdateContentDto dto)
        {
            var content = await _context.Contents
                .Include(c => c.Files)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (content == null) return null;

            _mapper.Map(dto, content);
            content.UpdatedAt = DateTime.UtcNow;

            // Delete files if specified
            if (dto.FilesToDelete != null && dto.FilesToDelete.Any())
            {
                var filesToDelete = content.Files.Where(f => dto.FilesToDelete.Contains(f.Id)).ToList();
                foreach (var file in filesToDelete)
                {
                    content.Files.Remove(file);
                    _context.Files.Remove(file);
                }
            }

            // Update thumbnail if provided
            if (dto.ThumbnailFile != null)
            {
                // Remove old thumbnail first to avoid unique constraint violation
                var oldThumbnail = content.Files.FirstOrDefault(f => f.Purpose == FilePurpose.thumbnail);
                if (oldThumbnail != null)
                {
                    _context.Files.Remove(oldThumbnail);
                    // Save changes to ensure old thumbnail is deleted before adding new one
                    await _context.SaveChangesAsync();
                }

                // Add new thumbnail
                var thumbnailUrl = await UploadImageAsync(dto.ThumbnailFile, "drawing-marketplace/content/thumbnails");
                var thumbnailFile = new MediaFile
                {
                    Id = Guid.NewGuid(),
                    ContentId = content.Id,
                    FileName = dto.ThumbnailFile.FileName,
                    FileUrl = thumbnailUrl,
                    FileType = dto.ThumbnailFile.ContentType,
                    Size = dto.ThumbnailFile.Length,
                    Purpose = FilePurpose.thumbnail,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Files.Add(thumbnailFile);
            }

            // Add new preview images if provided
            if (dto.PreviewFiles != null && dto.PreviewFiles.Any())
            {
                var maxOrder = content.Files.Where(f => f.Purpose == FilePurpose.preview)
                    .Select(f => f.DisplayOrder)
                    .DefaultIfEmpty(-1)
                    .Max();
                int order = maxOrder + 1;
                foreach (var previewFile in dto.PreviewFiles)
                {
                    var previewUrl = await UploadImageAsync(previewFile, "drawing-marketplace/content/previews");
                    var mediaFile = new MediaFile
                    {
                        Id = Guid.NewGuid(),
                        ContentId = content.Id,
                        FileName = previewFile.FileName,
                        FileUrl = previewUrl,
                        FileType = previewFile.ContentType,
                        Size = previewFile.Length,
                        Purpose = FilePurpose.preview,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Files.Add(mediaFile);
                }
            }

            // Add new downloadable files if provided
            if (dto.DownloadableFiles != null && dto.DownloadableFiles.Any())
            {
                var maxOrder = content.Files.Where(f => f.Purpose == FilePurpose.downloadable)
                    .Select(f => f.DisplayOrder)
                    .DefaultIfEmpty(-1)
                    .Max();
                int order = maxOrder + 1;
                foreach (var downloadFile in dto.DownloadableFiles)
                {
                    var downloadUrl = await UploadFileAsync(downloadFile, "drawing-marketplace/content/downloads");
                    var mediaFile = new MediaFile
                    {
                        Id = Guid.NewGuid(),
                        ContentId = content.Id,
                        FileName = downloadFile.FileName,
                        FileUrl = downloadUrl,
                        FileType = downloadFile.ContentType,
                        Size = downloadFile.Length,
                        Purpose = FilePurpose.downloadable,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Files.Add(mediaFile);
                }
            }

            await _context.SaveChangesAsync();

            return await GetManagementDetailAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var content = await _context.Contents.FindAsync(id);
            if (content == null) return false;

            _context.Contents.Remove(content);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ContentDetailDto?> ApproveContentAsync(Guid contentId, bool publish)
        {
            var content = await _context.Contents.FindAsync(contentId);
            if (content == null) return null;

            content.Status = publish ? ContentStatus.published : ContentStatus.archived;
            content.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetManagementDetailAsync(contentId);
        }

        public async Task<Content?> GetEntityByIdAsync(Guid contentId, CancellationToken ct = default)
        {
            return await _context.Contents
                .Include(c => c.Files)
                .FirstOrDefaultAsync(c => c.Id == contentId, ct);
        }

        private async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File rỗng");

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder,
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null)
                throw new Exception($"Lỗi upload ảnh: {result.Error.Message}");

            return result.SecureUrl.ToString();
        }

        private async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File rỗng");

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null)
                throw new Exception($"Lỗi upload file: {result.Error.Message}");

            return result.SecureUrl.ToString();
        }
    }
}