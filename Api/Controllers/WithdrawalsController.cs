using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Withdrawal;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/withdrawals")]
    [Authorize]
    public class WithdrawalsController : ControllerBase
    {
        private readonly IWithdrawalService _withdrawalService;
        private readonly DrawingMarketplaceContext _context;

        public WithdrawalsController(
            IWithdrawalService withdrawalService,
            DrawingMarketplaceContext context)
        {
            _withdrawalService = withdrawalService;
            _context = context;
        }

        [SwaggerOperation(
            Summary = "Tạo yêu cầu rút tiền",
            Description = "Collaborator tạo yêu cầu rút tiền từ ví"
        )]
        [Authorize(Roles = "collaborator")]
        [HttpPost]
        public async Task<IActionResult> CreateWithdrawal([FromBody] CreateWithdrawalDto dto)
        {
            var withdrawal = await _withdrawalService.CreateWithdrawalAsync(dto);
            return this.Success(withdrawal, "Tạo yêu cầu rút tiền thành công", "Create withdrawal successfully", 201);
        }

        [SwaggerOperation(
            Summary = "Lấy yêu cầu rút tiền của tôi",
            Description = "Lấy danh sách yêu cầu rút tiền của collaborator đang đăng nhập"
        )]
        [Authorize(Roles = "collaborator")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyWithdrawals()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return this.Fail("Token không hợp lệ", "Invalid token", 401);

            var collaborator = await _context.Collaborators
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (collaborator == null)
                return this.NotFound("Collaborator", "Collaborator not found");

            var withdrawals = await _withdrawalService.GetCollaboratorWithdrawalsAsync(collaborator.Id);
            return this.Success(withdrawals, "Lấy danh sách yêu cầu rút tiền thành công", "Get withdrawals successfully");
        }

        [SwaggerOperation(
        Summary = "Danh sách rút tiền chờ duyệt",
        Description = "Admin lấy danh sách các yêu cầu rút tiền đang chờ duyệt"
    )]
        [Authorize(Roles = "admin")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingWithdrawals()
        {
            var withdrawals = await _withdrawalService.GetPendingWithdrawalsAsync();
            return this.Success(withdrawals, "Lấy danh sách yêu cầu rút tiền chờ duyệt thành công", "Get pending withdrawals successfully");
        }

        [SwaggerOperation(
            Summary = "Danh sách tất cả yêu cầu rút tiền",
            Description = "Admin lấy danh sách tất cả yêu cầu rút tiền, có filter theo trạng thái và thời gian"
        )]
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllWithdrawals(
            [FromQuery] WithdrawalStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var withdrawals = await _withdrawalService.GetAllWithdrawalsAsync(status, fromDate, toDate, page, pageSize);
            return this.Success(withdrawals, "Lấy danh sách tất cả yêu cầu rút tiền thành công", "Get all withdrawals successfully");
        }

        [SwaggerOperation(
            Summary = "Duyệt yêu cầu rút tiền",
            Description = "Admin phê duyệt yêu cầu rút tiền"
        )]
        [Authorize(Roles = "admin")]
        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> ApproveWithdrawal(Guid id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out var adminId))
                return this.Fail("Token không hợp lệ", "Invalid token", 401);

            var withdrawal = await _withdrawalService.ApproveWithdrawalAsync(id, adminId);
            return this.Success(withdrawal, "Duyệt yêu cầu rút tiền thành công", "Approve withdrawal successfully");
        }

        [SwaggerOperation(
            Summary = "Từ chối yêu cầu rút tiền",
            Description = "Admin từ chối yêu cầu rút tiền, có thể kèm lý do"
        )]
        [Authorize(Roles = "admin")]
        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> RejectWithdrawal(Guid id, [FromBody] string? reason = null)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out var adminId))
                return this.Fail("Token không hợp lệ", "Invalid token", 401);

            var withdrawal = await _withdrawalService.RejectWithdrawalAsync(id, adminId, reason);
            return this.Success(withdrawal, "Từ chối yêu cầu rút tiền thành công", "Reject withdrawal successfully");
        }

        [SwaggerOperation(
           Summary = "Đánh dấu đã thanh toán",
           Description = "Admin đánh dấu yêu cầu rút tiền đã được thanh toán"
       )]
        [Authorize(Roles = "admin")]
        [HttpPost("{id:guid}/paid")]
        public async Task<IActionResult> MarkAsPaid(Guid id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out var adminId))
                return this.Fail("Token không hợp lệ", "Invalid token", 401);

            var withdrawal = await _withdrawalService.MarkAsPaidAsync(id, adminId);
            return this.Success(withdrawal, "Đánh dấu đã thanh toán thành công", "Mark as paid successfully");
        }
    }
}