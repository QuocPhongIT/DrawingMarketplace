using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Application.DTOs.Pagination;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static DrawingMarketplace.Application.DTOs.Content.ContentUpsertDto;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/contents")]
    public class ContentsController : ControllerBase
    {
        private readonly IContentService _service;
        private readonly ICurrentUserService _currentUserService;

        public ContentsController(IContentService service, ICurrentUserService currentUserService)
        {
            _service = service;
            _currentUserService = currentUserService;
        }

        [SwaggerOperation(
            Summary = "Danh sách content công khai",
            Description = "Lấy danh sách content đã được publish, hỗ trợ phân trang, tìm kiếm, lọc và sắp xếp"
        )]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetPublic(
             int page = 1,
             int pageSize = 10,
             string? keyword = null,
             string? categoryName = null,
             decimal? minPrice = null,
             decimal? maxPrice = null,
             ContentSortBy sortBy = ContentSortBy.Newest,
             SortDirection sortDir = SortDirection.Desc)
        { 
            var result = await _service.GetPagedPublicAsync(
                page,
                pageSize,
                keyword,
                categoryName,
                minPrice,
                maxPrice,
                sortBy,
                sortDir
            );

            return this.Success(result, "Lấy danh sách content thành công", "Get content list successfully");
        }

        [SwaggerOperation(
            Summary = "Danh sách content quản trị",
            Description = "Admin lấy danh sách toàn bộ content, có thể lọc theo trạng thái và collaborator"
        )]
        [Authorize(Roles = "admin")]
        [HttpGet("management")]
        public async Task<IActionResult> GetManagement(
             int page = 1,
             int pageSize = 10,
             string? keyword = null,
             string? categoryName = null,
             ContentStatus? status = null,
             Guid? collaboratorId = null,
             ContentSortBy sortBy = ContentSortBy.Newest,
             SortDirection sortDir = SortDirection.Desc)
        { 
            var result = await _service.GetPagedManagementAsync(
                page,
                pageSize,
                keyword,
                categoryName,
                status,
                collaboratorId,
                sortBy,
                sortDir
            );

            return this.Success(result, "Lấy danh sách content quản trị thành công", "Get management content list successfully");
        }

        [SwaggerOperation(
            Summary = "Danh sách content đã mua",
            Description = "Người dùng lấy danh sách content đã mua của chính mình"
        )]
        [Authorize]
        [HttpGet("mypurchases")]
        public async Task<IActionResult> GetMyPurchases(
             int page = 1,
             int pageSize = 10,
             string? keyword = null,
             string? categoryName = null,
             ContentSortBy sortBy = ContentSortBy.Newest,
             SortDirection sortDir = SortDirection.Desc)
        {
            if (!_currentUserService.UserId.HasValue)
                return this.Unauthorized();

            var result = await _service.GetPagedMyPurchasesAsync(
                _currentUserService.UserId.Value,
                page,
                pageSize,
                keyword,
                categoryName,
                sortBy,
                sortDir
            );

            return this.Success(result, "Lấy danh sách content đã mua thành công", "Get purchased content list successfully");
        }

        [SwaggerOperation(
            Summary = "Chi tiết content công khai",
            Description = "Lấy chi tiết content đã publish theo ID"
        )]
        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var content = await _service.GetPublicDetailAsync(id);
            if (content == null)
                return this.NotFound("Content", "Content not found");
            
            return this.Success(content, "Lấy chi tiết content thành công", "Get content detail successfully");
        }

        [SwaggerOperation(
            Summary = "Chi tiết content quản trị",
            Description = "Admin xem chi tiết content bao gồm dữ liệu quản trị"
        )]
        [Authorize(Roles = "admin")]
        [HttpGet("management/{id:guid}")]
        public async Task<IActionResult> GetManagementById(Guid id)
        {
            var content = await _service.GetManagementDetailAsync(id);
            if (content == null)
                return this.NotFound("Content", "Content not found");
            
            return this.Success(content, "Lấy chi tiết content quản trị thành công", "Get management content detail successfully");
        }

        [SwaggerOperation(
           Summary = "Tạo content mới",
           Description = "Collaborator hoặc Admin tạo content mới"
       )]
        [Authorize(Roles = "collaborator,admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateContentDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return this.Success(
                created, 
                "Tạo content thành công", 
                "Create content successfully",
                201
            );
        }

        [SwaggerOperation(
           Summary = "Cập nhật content",
           Description = "Cập nhật thông tin content theo ID"
       )]
        [Authorize(Roles = "collaborator,admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateContentDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return this.NotFound("Content", "Content not found");
            
            return this.Success(updated, "Cập nhật content thành công", "Update content successfully");
        }

        [SwaggerOperation(
            Summary = "Xóa content",
            Description = "Admin xóa content theo ID"
        )]
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return this.NotFound("Content", "Content not found");
            
            return this.Success<object>(
                null,
                "Xóa content thành công",
                "Delete content successfully"
            );
        }

        [SwaggerOperation(
            Summary = "Duyệt hoặc lưu trữ content",
            Description = "Admin duyệt (publish=true) hoặc lưu trữ (publish=false) content"
        )]
        [Authorize(Roles = "admin")]
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] bool publish)
        {
            var result = await _service.ApproveContentAsync(id, publish);
            if (result == null)
                return this.NotFound("Content", "Content not found");
            
            var message = publish 
                ? "Phê duyệt content thành công" 
                : "Lưu trữ content thành công";
            var messageEn = publish 
                ? "Approve content successfully" 
                : "Archive content successfully";
            
            return this.Success(result, message, messageEn);
        }
    }
}
