using DrawingMarketplace.Api.Extensions; 
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace DrawingMarketplace.Api.Controllers
{
    [Route("api/contentstats")]
    [ApiController]
    public class ContentStatsController : ControllerBase
    {
        private readonly IContentStatsService _contentStatsService;

        public ContentStatsController(IContentStatsService contentStatsService)
        {
            _contentStatsService = contentStatsService;
        }

        [SwaggerOperation(
            Summary = "Thống kê content của collaborator",
            Description = "Collaborator lấy thống kê các content của chính mình, hỗ trợ phân trang và tìm kiếm"
        )]
        [Authorize(Roles = "collaborator")]
        [HttpGet]
        public async Task<IActionResult> GetMyStats(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return this.Fail("Không được phép truy cập", "Unauthorized", 401);

            var result = await _contentStatsService.GetMyStatsAsync(userId, page, pageSize, keyword);

            return this.Success(result, "Lấy thống kê nội dung của bạn thành công", "Get your content statistics successfully");
        }

        [SwaggerOperation(
            Summary = "Thống kê tất cả content",
            Description = "Admin xem thống kê toàn bộ content, có thể lọc theo collaborator và sắp xếp"
        )]
        [Authorize(Roles = "admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllStats(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] Guid? collaboratorId = null,
            [FromQuery] ContentSortBy sortBy = ContentSortBy.Sold,
            [FromQuery] SortDirection sortDir = SortDirection.Desc)
        {
            var result = await _contentStatsService.GetAllStatsAsync(
                page, pageSize, keyword, collaboratorId, sortBy, sortDir);

            return this.Success(result, "Lấy thống kê tất cả nội dung thành công", "Get all content statistics successfully");
        }
    }
}