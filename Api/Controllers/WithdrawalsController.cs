using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Withdrawal;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/withdrawals")]
    [Authorize]
    public class WithdrawalsController : ControllerBase
    {
        private readonly IWithdrawalService _withdrawalService;

        public WithdrawalsController(IWithdrawalService withdrawalService)
        {
            _withdrawalService = withdrawalService;
        }

        [HttpPost]
        [Authorize(Roles = "collaborator")]
        public async Task<IActionResult> CreateWithdrawal([FromBody] CreateWithdrawalDto dto)
        {
            var withdrawal = await _withdrawalService.CreateWithdrawalAsync(dto);
            return this.Success(withdrawal, "Tạo yêu cầu rút tiền thành công", "Create withdrawal successfully", 201);
        }

        [HttpGet("my")]
        [Authorize(Roles = "collaborator")]
        public async Task<IActionResult> GetMyWithdrawals([FromServices] DrawingMarketplace.Infrastructure.Persistence.DrawingMarketplaceContext context)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var collaborator = await context.Collaborators.FirstOrDefaultAsync(c => c.UserId == userId);
            if (collaborator == null)
                return this.NotFound("Collaborator", "Collaborator not found");
            
            var withdrawals = await _withdrawalService.GetCollaboratorWithdrawalsAsync(collaborator.Id);
            return this.Success(withdrawals, "Lấy danh sách yêu cầu rút tiền thành công", "Get withdrawals successfully");
        }

        [HttpGet("pending")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetPendingWithdrawals()
        {
            var withdrawals = await _withdrawalService.GetPendingWithdrawalsAsync();
            return this.Success(withdrawals, "Lấy danh sách yêu cầu rút tiền chờ duyệt thành công", "Get pending withdrawals successfully");
        }

        [HttpPost("{id:guid}/approve")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ApproveWithdrawal(Guid id)
        {
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var withdrawal = await _withdrawalService.ApproveWithdrawalAsync(id, adminId);
            return this.Success(withdrawal, "Duyệt yêu cầu rút tiền thành công", "Approve withdrawal successfully");
        }

        [HttpPost("{id:guid}/reject")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RejectWithdrawal(Guid id)
        {
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var withdrawal = await _withdrawalService.RejectWithdrawalAsync(id, adminId);
            return this.Success(withdrawal, "Từ chối yêu cầu rút tiền thành công", "Reject withdrawal successfully");
        }
    }
}

