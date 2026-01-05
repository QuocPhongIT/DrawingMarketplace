using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Collaborator;
using DrawingMarketplace.Application.Features.Collaborators;
using DrawingMarketplace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/collaborators")]
    public sealed class CollaboratorController : ControllerBase
    {
        private readonly ApplyCollaboratorHandler _apply;
        private readonly ApproveCollaboratorHandler _approve;
        private readonly RejectCollaboratorHandler _reject;
        private readonly GetAllCollaboratorsHandler _getAll;

        public CollaboratorController(
            ApplyCollaboratorHandler apply,
            ApproveCollaboratorHandler approve,
            RejectCollaboratorHandler reject,
            GetAllCollaboratorsHandler getAll)
        {
            _apply = apply;
            _approve = approve;
            _reject = reject;
            _getAll = getAll;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Apply()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _apply.ExecuteAsync(userId);
            return this.Success<object>(
                null,
                "Gửi đơn đăng ký collaborator thành công",
                "Apply collaborator successfully",
                202
            );
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _getAll.ExecuteAsync();
            return this.Success(
                result,
                "Lấy danh sách collaborator thành công",
                "Get collaborator list successfully"
            );
        }

        [Authorize(Roles = "admin")]
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateCollaboratorStatusRequest request)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ct = HttpContext.RequestAborted;

            return request.Status switch
            {
                CollaboratorRequestStatus.approved => await Approve(id, adminId, ct),
                CollaboratorRequestStatus.rejected => await Reject(id, adminId, ct),
                _ => this.Fail("Trạng thái không hợp lệ", "Invalid status", 400)
            };
        }

        private async Task<IActionResult> Approve(Guid id, Guid adminId, CancellationToken ct)
        {
            await _approve.ExecuteAsync(id, adminId, ct);
            return this.Success<object>(
                null,
                "Phê duyệt collaborator thành công",
                "Approve collaborator successfully"
            );
        }

        private async Task<IActionResult> Reject(Guid id, Guid adminId, CancellationToken ct)
        {
            await _reject.ExecuteAsync(id, adminId, ct);
            return this.Success<object>(
                null,
                "Từ chối collaborator thành công",
                "Reject collaborator successfully"
            );
        }
    }
}
