using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using DrawingMarketplace.Application.DTOs.Banner;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DrawingMarketplace.Domain.Exceptions;

namespace DrawingMarketplace.Application.Services
{
    public class BannerService : IBannerService
    {
        private readonly DrawingMarketplaceContext _context;
        private readonly Cloudinary _cloudinary;

        public BannerService(DrawingMarketplaceContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<List<BannerDto>> GetActiveBannersAsync()
        {
            return await _context.Banners
                .AsNoTracking()
                .Where(b => b.IsActive && b.DeletedAt == null)
                .OrderBy(b => b.DisplayOrder)
                .ThenBy(b => b.CreatedAt)
                .Select(b => MapToDto(b))
                .ToListAsync();
        }

        public async Task<List<BannerDto>> GetAllBannersAsync()
        {
            return await _context.Banners
                .AsNoTracking()
                .Where(b => b.DeletedAt == null)
                .OrderBy(b => b.DisplayOrder)
                .Select(b => MapToDto(b))
                .ToListAsync();
        }

        public async Task<BannerDto> CreateBannerAsync(CreateBannerDto dto, Guid currentUserId)
        {
            var imageUrl = await UploadImageAsync(dto.ImageFile);

            var banner = new Banner
            {
                Title = dto.Title,
                Subtitle = dto.Subtitle,
                ImageUrl = imageUrl,
                Button1Text = dto.Button1Text,
                Button1Link = dto.Button1Link,
                Button2Text = dto.Button2Text,
                Button2Link = dto.Button2Link,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                CreatedBy = currentUserId,
                UpdatedBy = currentUserId
            };

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            return MapToDto(banner);
        }

        public async Task<BannerDto> UpdateBannerAsync(Guid id, UpdateBannerDto dto, Guid currentUserId)
        {
            var banner = await _context.Banners
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null)
                ?? throw new NotFoundException("Banner", id);

            banner.Title = dto.Title;
            banner.Subtitle = dto.Subtitle;
            banner.Button1Text = dto.Button1Text;
            banner.Button1Link = dto.Button1Link;
            banner.Button2Text = dto.Button2Text;
            banner.Button2Link = dto.Button2Link;
            banner.IsActive = dto.IsActive ?? banner.IsActive;
            banner.DisplayOrder = dto.DisplayOrder ?? banner.DisplayOrder;
            banner.UpdatedBy = currentUserId;
            banner.UpdatedAt = DateTime.UtcNow;

            if (dto.ImageFile != null)
            {

                banner.ImageUrl = await UploadImageAsync(dto.ImageFile);
            }

            await _context.SaveChangesAsync();
            return MapToDto(banner);
        }

        public async Task DeleteBannerAsync(Guid id)
        {
            var banner = await _context.Banners
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null)
                ?? throw new NotFoundException("Banner", id);


            banner.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("File ảnh không hợp lệ");

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = "drawing-marketplace/banners",
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new BadRequestException($"Upload ảnh thất bại: {result.Error.Message}");

            if (result.SecureUrl == null)
                throw new BadRequestException("Không lấy được URL ảnh sau khi upload");

            return result.SecureUrl.ToString();
        }

        private static BannerDto MapToDto(Banner banner) => new()
        {
            Id = banner.Id,
            Title = banner.Title,
            Subtitle = banner.Subtitle,
            ImageUrl = banner.ImageUrl,
            Button1Text = banner.Button1Text,
            Button1Link = banner.Button1Link,
            Button2Text = banner.Button2Text,
            Button2Link = banner.Button2Link,
            IsActive = banner.IsActive,
            DisplayOrder = banner.DisplayOrder,
            CreatedAt = banner.CreatedAt
        };
    }
}
