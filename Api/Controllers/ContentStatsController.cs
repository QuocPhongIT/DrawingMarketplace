using DrawingMarketplace.Api.Extensions; 
using DrawingMarketplace.Api.Responses;  
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DrawingMarketplace.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentStatsController : ControllerBase
    {
        private readonly IContentStatsService _contentStatsService;

        public ContentStatsController(IContentStatsService contentStatsService)
        {
            _contentStatsService = contentStatsService;
        }
        [Authorize(Roles = "collaborator")]
        [HttpGet]
        public async Task<IActionResult> GetMyStats(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return ResponseHelper.ErrorResponse(this, 401, "Không được phép truy cập", "Unauthorized");

            var userId = Guid.Parse(userIdClaim.Value);

            var result = await _contentStatsService.GetMyStatsAsync(userId, page, pageSize, keyword);

            return ResponseHelper.SuccessResponse(
                controller: this,
                statusCode: 200,
                message: "Lấy thống kê nội dung của bạn thành công",
                data: result,
                messageEn: "Get your content statistics successfully");
        }
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

            return ResponseHelper.SuccessResponse(
                controller: this,
                statusCode: 200,
                message: "Lấy thống kê tất cả nội dung thành công",
                data: result,
                messageEn: "Get all content statistics successfully");
        }
    }
}