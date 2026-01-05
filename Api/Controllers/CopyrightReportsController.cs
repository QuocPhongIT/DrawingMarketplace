using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.CopyrightReport;
using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/copyright-reports")]
    public sealed class CopyrightReportsController : ControllerBase
    {
        private readonly ICopyrightReportService _service;

        public CopyrightReportsController(
            ICopyrightReportService service)
        {
            _service = service;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(
            [FromBody] CreateCopyrightReportRequest request)
        {
            await _service.CreateAsync(request);

            return this.Success<object>(
                null,
                "Gửi report bản quyền thành công",
                "Submit copyright report successfully",
                201
            );
        }
        [HttpGet("management")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _service.GetAllAsync();

            return this.Success(
                reports,
                "Lấy danh sách report thành công",
                "Get copyright report list successfully"
            );
        }

        [HttpGet("management/{id:guid}")]
        [Authorize(Roles = "admin")]
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
        [HttpPatch("{id:guid}/approve")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var success = await _service.ApproveAsync(id);
            if (!success)
                return this.NotFound(
                    "CopyrightReport",
                    "Copyright report not found"
                );

            return this.Success<object>(
                null,
                "Phê duyệt report thành công",
                "Approve report successfully"
            );
        }
        [HttpPatch("{id:guid}/reject")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var success = await _service.RejectAsync(id);
            if (!success)
                return this.NotFound(
                    "CopyrightReport",
                    "Copyright report not found"
                );

            return this.Success<object>(
                null,
                "Từ chối report thành công",
                "Reject report successfully"
            );
        }
    }
}
