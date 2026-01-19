using CloudinaryDotNet;
using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Banner;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace DrawingMarketplace.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BannersController : ControllerBase
    {
        private readonly IBannerService _bannerService;

        public BannersController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        [SwaggerOperation(
            Summary = "Lấy banner đang hoạt động",
            Description = "API public dùng cho trang chủ"
        )]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetActiveBanners()
        {
            var banners = await _bannerService.GetActiveBannersAsync();
            return this.Success(banners, "Lấy danh sách banner thành công", "Get active banners successfully");
        }

        [SwaggerOperation(
            Summary = "Lấy tất cả banner",
            Description = "Chỉ admin sử dụng"
        )]
        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllBanners()
        {
            var banners = await _bannerService.GetAllBannersAsync();
            return this.Success(banners, "Lấy tất cả banner thành công", "Get all banners successfully");
        }

        [SwaggerOperation(
            Summary = "Tạo banner",
            Description = "Upload ảnh + thông tin banner"
        )]
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateBanner([FromForm] CreateBannerDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var banner = await _bannerService.CreateBannerAsync(dto, userId);
            return this.Success(banner, "Tạo banner thành công", "Create banner successfully", 201);
        }

        [SwaggerOperation(
            Summary = "Cập nhật banner",
            Description = "Cập nhật thông tin hoặc ảnh banner"
        )]
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateBanner(Guid id, [FromForm] UpdateBannerDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _bannerService.UpdateBannerAsync(id, dto, userId);
            return this.Success<object>(null, "Cập nhật banner thành công", "Update banner successfully");
        }

        [SwaggerOperation(
           Summary = "Xóa banner",
           Description = "Xóa banner theo id"
       )]
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBanner(Guid id)
        {
            await _bannerService.DeleteBannerAsync(id);
            return this.Success<object>(null, "Xóa banner thành công", "Delete banner successfully");
        }

        [SwaggerOperation(
            Summary = "Lấy chi tiết banner",
            Description = "Admin xem chi tiết banner"
        )]
        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]        
        public async Task<IActionResult> GetBannerById(Guid id)
        {
            var banners = await _bannerService.GetAllBannersAsync();
            var banner = banners.FirstOrDefault(b => b.Id == id);
            if (banner == null)
                return this.NotFound("Banner", "Banner not found");

            return this.Success(banner, "Lấy chi tiết banner thành công", "Get banner detail successfully");
        }
    }
}