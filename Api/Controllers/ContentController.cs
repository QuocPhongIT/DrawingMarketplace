using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Application.DTOs.Pagination;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static DrawingMarketplace.Application.DTOs.Content.ContentUpsertDto;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/contents")]
    public class ContentsController : ControllerBase
    {
        private readonly IContentService _service;

        public ContentsController(IContentService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
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

        [HttpGet("management")]
        [Authorize]
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

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var content = await _service.GetPublicDetailAsync(id);
            if (content == null)
                return this.NotFound("Content", "Content not found");
            
            return this.Success(content, "Lấy chi tiết content thành công", "Get content detail successfully");
        }

        [HttpGet("management/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetManagementById(Guid id)
        {
            var content = await _service.GetManagementDetailAsync(id);
            if (content == null)
                return this.NotFound("Content", "Content not found");
            
            return this.Success(content, "Lấy chi tiết content quản trị thành công", "Get management content detail successfully");
        }

        [HttpPost]
        [Authorize]
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

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateContentDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return this.NotFound("Content", "Content not found");
            
            return this.Success(updated, "Cập nhật content thành công", "Update content successfully");
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
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

        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "admin")]
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
