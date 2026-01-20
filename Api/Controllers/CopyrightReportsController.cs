using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.CopyrightReport;
using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/copyrightreports")]
    public sealed class CopyrightReportsController : ControllerBase
    {
        private readonly ICopyrightReportService _service;

        public CopyrightReportsController(ICopyrightReportService service)
        {
            _service = service;
        }

        [SwaggerOperation(
            Summary = "Gửi report vi phạm bản quyền",
            Description = "Người dùng gửi báo cáo vi phạm bản quyền cho nội dung"
        )]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCopyrightReportRequest request)
        {
            await _service.CreateAsync(request);
            return this.Success<object>(
                null,
                "Gửi report bản quyền thành công",
                "Submit copyright report successfully",
                201
            );
        }

        [SwaggerOperation(
            Summary = "Danh sách report bản quyền",
            Description = "Admin xem danh sách tất cả report vi phạm bản quyền"
        )]
        [Authorize(Roles = "admin")]
        [HttpGet("management")]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _service.GetAllAsync();
            return this.Success(
                reports,
                "Lấy danh sách report thành công",
                "Get copyright report list successfully"
            );
        }

        [SwaggerOperation(
            Summary = "Chi tiết report bản quyền",
            Description = "Admin xem chi tiết một report vi phạm bản quyền theo ID"
        )]
        [Authorize(Roles = "admin")]
        [HttpGet("management/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var report = await _service.GetByIdAsync(id);
            if (report == null)
                return this.NotFound(
                    "CopyrightReport",
                    "Copyright report not found"
                );

            return this.Success(
                report,
                "Lấy chi tiết report thành công",
                "Get copyright report detail successfully"
            );
        }

        [SwaggerOperation(
            Summary = "Phê duyệt report bản quyền",
            Description = "Admin phê duyệt report vi phạm bản quyền"
        )]
        [Authorize(Roles = "admin")]
        [HttpPatch("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var success = await _service.ApproveAsync(id);
            if (!success)
                return this.NotFound(
                    "CopyrightReport",
                    "Copyright report not found or already processed"
                );

            return this.Success<object>(
                null,
                "Phê duyệt report thành công",
                "Approve report successfully"
            );
        }

        [SwaggerOperation(
            Summary = "Từ chối report bản quyền",
            Description = "Admin từ chối report vi phạm bản quyền"
        )]
        [Authorize(Roles = "admin")]
        [HttpPatch("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var success = await _service.RejectAsync(id);
            if (!success)
                return this.NotFound(
                    "CopyrightReport",
                    "Copyright report not found or already processed"
                );

            return this.Success<object>(
                null,
                "Từ chối report thành công",
                "Reject report successfully"
            );
        }
    }
}