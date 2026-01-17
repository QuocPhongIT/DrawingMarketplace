using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Application.DTOs.MediaFile;
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
                    x.ContentStat != null ? x.ContentStat.Purchases ?? 0 : 0),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(c => c.Files)
                .Include(c => c.ContentStat)
                .Include(c => c.Collaborator)
                .ThenInclude(col => col.User)
                .Select(c => new ContentListDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    Status = c.Status,
                    CategoryId = c.CategoryId,
                    CreatedAt = c.CreatedAt.GetValueOrDefault(),
                    CollaboratorId = c.CollaboratorId.HasValue ? c.CollaboratorId.Value : Guid.Empty,
                    CollaboratorUsername = c.Collaborator != null && c.Collaborator.User != null ? c.Collaborator.User.Username : string.Empty,
                    ThumbnailUrl = c.Files
                        .Where(f => f.Purpose == FilePurpose.thumbnail)
                        .Select(f => f.FileUrl)
                        .FirstOrDefault(),
                    PreviewUrls = c.Files
                        .Where(f => f.Purpose == FilePurpose.preview)
                        .OrderBy(f => f.DisplayOrder)
                        .ThenBy(f => f.CreatedAt)
                        .Select(f => f.FileUrl)
                        .Take(4)
                        .ToList(),
                    Views = c.ContentStat != null ? c.ContentStat.Views ?? 0 : 0,
                    Purchases = c.ContentStat != null ? c.ContentStat.Purchases ?? 0 : 0,
                    Downloads = c.ContentStat != null ? c.ContentStat.Downloads ?? 0 : 0
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
            var updated = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE content_stats SET views = views + 1 WHERE content_id = {0}", contentId);

            if (updated == 0)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO content_stats (content_id, views, downloads, purchases)
                      VALUES ({0}, 1, 0, 0)
                      ON CONFLICT (content_id) DO NOTHING",
                    contentId);
            }

            return await _context.Contents
                .AsNoTracking()
                .Where(c => c.Id == contentId && c.Status == ContentStatus.published && c.DeletedAt == null)
                .Select(c => new ContentDetailDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    Status = c.Status,
                    CategoryId = c.CategoryId,
                    CollaboratorId = c.CollaboratorId,
                    CreatedAt = c.CreatedAt.GetValueOrDefault(),
                    UpdatedAt = c.UpdatedAt,
                    Stats = c.ContentStat != null
                        ? new ContentStatsDto
                        {
                            Views = c.ContentStat.Views ?? 0,
                            Purchases = c.ContentStat.Purchases ?? 0,
                            Downloads = c.ContentStat.Downloads ?? 0
                        }
                        : new ContentStatsDto { Views = 1, Purchases = 0, Downloads = 0 },
                    ThumbnailUrl = c.Files
                        .Where(f => f.Purpose == FilePurpose.thumbnail)
                        .Select(f => f.FileUrl)
                        .FirstOrDefault(),
                    PreviewFiles = c.Files
                        .Where(f => f.Purpose == FilePurpose.preview)
                        .OrderBy(f => f.DisplayOrder)
                        .ThenBy(f => f.CreatedAt)
                        .Select(f => new MediaFileDto
                        {
                            Id = f.Id,
                            FileName = f.FileName,
                            FileUrl = f.FileUrl,
                            FileType = f.FileType,
                            Size = f.Size
                        })
                        .ToList(),
                    DownloadableFiles = c.Files
                        .Where(f => f.Purpose == FilePurpose.downloadable)
                        .OrderBy(f => f.DisplayOrder)
                        .ThenBy(f => f.CreatedAt)
                        .Select(f => new MediaFileDto
                        {
                            Id = f.Id,
                            FileName = f.FileName,
                            FileUrl = null,
                            FileType = f.FileType,
                            Size = f.Size,
                            PublicId = f.PublicId
                        })
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
            var query = _context.Contents.AsNoTracking();

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
                    x.ContentStat != null ? x.ContentStat.Purchases ?? 0 : 0),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(c => c.Files)
                .Select(c => new ContentListDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    Status = c.Status,
                    CategoryId = c.CategoryId,
                    CreatedAt = c.CreatedAt.GetValueOrDefault(),
                    ThumbnailUrl = c.Files
                        .Where(f => f.Purpose == FilePurpose.thumbnail)
                        .Select(f => f.FileUrl)
                        .FirstOrDefault(),
                    PreviewUrls = c.Files
                        .Where(f => f.Purpose == FilePurpose.preview)
                        .OrderBy(f => f.DisplayOrder)
                        .ThenBy(f => f.CreatedAt)
                        .Select(f => f.FileUrl)
                        .Take(4)
                        .ToList(),
                    CollaboratorId = c.CollaboratorId ?? Guid.Empty,
                    CollaboratorUsername = c.Collaborator != null && c.Collaborator.User != null
                        ? c.Collaborator.User.Username ?? ""
                        : "",
                    Views = c.ContentStat != null ? c.ContentStat.Views ?? 0 : 0,
                    Purchases = c.ContentStat != null ? c.ContentStat.Purchases ?? 0 : 0,
                    Downloads = c.ContentStat != null ? c.ContentStat.Downloads ?? 0 : 0
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                    CreatedAt = c.CreatedAt.GetValueOrDefault(),
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
                        .FirstOrDefault(),
                    PreviewFiles = c.Files
                        .Where(f => f.Purpose == FilePurpose.preview)
                        .OrderBy(f => f.DisplayOrder)
                        .ThenBy(f => f.CreatedAt)
                        .Select(f => new MediaFileDto
                        {
                            Id = f.Id,
                            FileName = f.FileName,
                            FileUrl = f.FileUrl,
                            FileType = f.FileType,
                            Size = f.Size
                        })
                        .ToList(),
                    DownloadableFiles = c.Files
                        .Where(f => f.Purpose == FilePurpose.downloadable)
                        .OrderBy(f => f.DisplayOrder)
                        .ThenBy(f => f.CreatedAt)
                        .Select(f => new MediaFileDto
                        {
                            Id = f.Id,
                            FileName = f.FileName,
                            FileUrl = null,
                            FileType = f.FileType,
                            Size = f.Size,
                            PublicId = f.PublicId
                        })
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

            if (dto.ThumbnailFile != null)
            {
                var (publicId, url) = await UploadImageAsync(dto.ThumbnailFile, "content/thumbnails");
                _context.Files.Add(new MediaFile
                {
                    Id = Guid.NewGuid(),
                    ContentId = content.Id,
                    FileName = dto.ThumbnailFile.FileName,
                    FileUrl = url,
                    PublicId = publicId,
                    FileType = dto.ThumbnailFile.ContentType,
                    Size = dto.ThumbnailFile.Length,
                    Purpose = FilePurpose.thumbnail,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (dto.PreviewFiles != null)
            {
                int order = 0;
                foreach (var file in dto.PreviewFiles)
                {
                    var (publicId, url) = await UploadImageAsync(file, "content/previews");
                    _context.Files.Add(new MediaFile
                    {
                        Id = Guid.NewGuid(),
                        ContentId = content.Id,
                        FileName = file.FileName,
                        FileUrl = url,
                        PublicId = publicId,
                        FileType = file.ContentType,
                        Size = file.Length,
                        Purpose = FilePurpose.preview,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (dto.DownloadableFiles != null)
            {
                int order = 0;
                foreach (var file in dto.DownloadableFiles)
                {
                    var publicId = await UploadFileAsync(file, "drawing-marketplace/content/downloads");
                    _context.Files.Add(new MediaFile
                    {
                        Id = Guid.NewGuid(),
                        ContentId = content.Id,
                        FileName = file.FileName,
                        FileUrl = "LOCKED",
                        PublicId = publicId,
                        FileType = file.ContentType,
                        Size = file.Length,
                        Purpose = FilePurpose.downloadable,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return await GetManagementDetailAsync(content.Id)
                ?? throw new Exception("Create failed");
        }

        public async Task<ContentDetailDto?> UpdateAsync(Guid id, UpdateContentDto dto)
        {
            var content = await _context.Contents
                .Include(c => c.Files)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (content == null) return null;

            _mapper.Map(dto, content);
            content.UpdatedAt = DateTime.UtcNow;

            if (dto.ThumbnailFile != null)
            {
                var oldThumbnail = content.Files.FirstOrDefault(f => f.Purpose == FilePurpose.thumbnail);
                if (oldThumbnail != null)
                {
                    content.Files.Remove(oldThumbnail);
                    _context.Files.Remove(oldThumbnail);
                }

                var (publicId, url) = await UploadImageAsync(dto.ThumbnailFile, "drawing-marketplace/content/thumbnails");
                _context.Files.Add(new MediaFile
                {
                    Id = Guid.NewGuid(),
                    ContentId = content.Id,
                    FileName = dto.ThumbnailFile.FileName,
                    FileUrl = url,
                    PublicId = publicId,
                    FileType = dto.ThumbnailFile.ContentType,
                    Size = dto.ThumbnailFile.Length,
                    Purpose = FilePurpose.thumbnail,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var currentPreviews = content.Files.Where(f => f.Purpose == FilePurpose.preview).ToList();
            var previewsToRemove = currentPreviews
                .Where(f => !dto.KeepPreviewFileIds.Contains(f.Id))
                .ToList();

            foreach (var file in previewsToRemove)
            {
                content.Files.Remove(file);
                _context.Files.Remove(file);
            }

            if (dto.NewPreviewFiles != null && dto.NewPreviewFiles.Any())
            {
                int order = currentPreviews.Count - previewsToRemove.Count;
                foreach (var file in dto.NewPreviewFiles)
                {
                    var (publicId, url) = await UploadImageAsync(file, "drawing-marketplace/content/previews");
                    _context.Files.Add(new MediaFile
                    {
                        Id = Guid.NewGuid(),
                        ContentId = content.Id,
                        FileName = file.FileName,
                        FileUrl = url,
                        PublicId = publicId,
                        FileType = file.ContentType,
                        Size = file.Length,
                        Purpose = FilePurpose.preview,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (dto.PreviewFileOrder.Any())
            {
                var keptPreviews = content.Files
                    .Where(f => f.Purpose == FilePurpose.preview && dto.KeepPreviewFileIds.Contains(f.Id))
                    .ToList();

                for (int i = 0; i < dto.PreviewFileOrder.Count; i++)
                {
                    var fileId = dto.PreviewFileOrder[i];
                    var file = keptPreviews.FirstOrDefault(f => f.Id == fileId);
                    if (file != null)
                    {
                        file.DisplayOrder = i;
                    }
                }
            }

            var currentDownloads = content.Files.Where(f => f.Purpose == FilePurpose.downloadable).ToList();
            var downloadsToRemove = currentDownloads
                .Where(f => !dto.KeepDownloadableFileIds.Contains(f.Id))
                .ToList();

            foreach (var file in downloadsToRemove)
            {
                content.Files.Remove(file);
                _context.Files.Remove(file);
            }

            if (dto.NewDownloadableFiles != null && dto.NewDownloadableFiles.Any())
            {
                int order = currentDownloads.Count - downloadsToRemove.Count;
                foreach (var file in dto.NewDownloadableFiles)
                {
                    var publicId = await UploadFileAsync(file, "drawing-marketplace/content/downloads");
                    _context.Files.Add(new MediaFile
                    {
                        Id = Guid.NewGuid(),
                        ContentId = content.Id,
                        FileName = file.FileName,
                        FileUrl = "LOCKED",
                        PublicId = publicId,
                        FileType = file.ContentType,
                        Size = file.Length,
                        Purpose = FilePurpose.downloadable,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    });
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

        public async Task<PagedResultDto<ContentListDto>> GetPagedMyPurchasesAsync(
            Guid userId,
            int page,
            int pageSize,
            string? keyword = null,
            string? categoryName = null,
            ContentSortBy sortBy = ContentSortBy.Newest,
            SortDirection sortDir = SortDirection.Desc)
        {
            // Step 1: Lấy danh sách content IDs mà user đã mua
            var purchasedContentIds = await _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.UserId == userId && oi.Order.Status == OrderStatus.paid)
                .Select(oi => oi.ContentId)
                .Distinct()
                .ToListAsync();

            if (purchasedContentIds.Count == 0)
                return new PagedResultDto<ContentListDto> { Items = new List<ContentListDto>(), TotalCount = 0, Page = page, PageSize = pageSize };

            // Step 2: Query trên Contents với IDs đó
            IQueryable<Content> query = _context.Contents
                .AsNoTracking()
                .Where(c => purchasedContentIds.Contains(c.Id) && c.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{keyword}%"));

            if (!string.IsNullOrWhiteSpace(categoryName))
                query = query.Where(x => x.Category != null && EF.Functions.ILike(x.Category.Name, $"%{categoryName}%"));

            // Step 3: Sắp xếp
            query = sortBy switch
            {
                ContentSortBy.Price => sortDir == SortDirection.Asc
                    ? query.OrderBy(x => x.Price)
                    : query.OrderByDescending(x => x.Price),
                ContentSortBy.Title => sortDir == SortDirection.Asc
                    ? query.OrderBy(x => x.Title)
                    : query.OrderByDescending(x => x.Title),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            // Step 4: Fetch content IDs sau skip/take
            var pagedContentIds = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Id)
                .ToListAsync();

            // Step 5: Lấy quantities cho các content này
            var quantitiesDict = await _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.UserId == userId && oi.Order.Status == OrderStatus.paid && pagedContentIds.Contains(oi.ContentId))
                .GroupBy(oi => oi.ContentId)
                .Select(g => new { ContentId = g.Key, TotalQuantity = g.Sum(oi => oi.Quantity) })
                .ToDictionaryAsync(x => x.ContentId, x => x.TotalQuantity);

            // Step 6: Fetch full content details
            var items = await _context.Contents
                .AsNoTracking()
                .Where(c => pagedContentIds.Contains(c.Id))
                .Include(c => c.Files)
                .Include(c => c.Collaborator)
                .ThenInclude(col => col.User)
                .Include(c => c.ContentStat)
                .Select(c => new ContentListDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    Status = c.Status,
                    CategoryId = c.CategoryId,
                    CreatedAt = c.CreatedAt.GetValueOrDefault(),
                    CollaboratorId = c.CollaboratorId.HasValue ? c.CollaboratorId.Value : Guid.Empty,
                    CollaboratorUsername = c.Collaborator != null && c.Collaborator.User != null ? c.Collaborator.User.Username : string.Empty,
                    ThumbnailUrl = c.Files
                        .Where(f => f.Purpose == FilePurpose.thumbnail)
                        .Select(f => f.FileUrl)
                        .FirstOrDefault(),
                    PreviewUrls = c.Files
                        .Where(f => f.Purpose == FilePurpose.preview)
                        .OrderBy(f => f.DisplayOrder)
                        .ThenBy(f => f.CreatedAt)
                        .Select(f => f.FileUrl)
                        .Take(4)
                        .ToList(),
                    Views = c.ContentStat != null ? c.ContentStat.Views ?? 0 : 0,
                    Purchases = c.ContentStat != null ? c.ContentStat.Purchases ?? 0 : 0,
                    Downloads = c.ContentStat != null ? c.ContentStat.Downloads ?? 0 : 0,
                    Quantity = quantitiesDict.ContainsKey(c.Id) ? quantitiesDict[c.Id] : 1
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

        private async Task<(string PublicId, string Url)> UploadImageAsync(
             IFormFile file,
             string folder)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder,
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception(result.Error.Message);

            return (result.PublicId, result.SecureUrl.ToString());
        }


        private async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ hoặc rỗng.");

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder,
                Type = "private",  
                UseFilename = true,  
                UniqueFilename = false  
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Upload thất bại: {result.Error.Message}");
            file.OpenReadStream().Dispose();

            return result.PublicId;
        }

    }
}