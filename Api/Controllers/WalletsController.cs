using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/wallets")]
    [Authorize(Roles = "collaborator")]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [SwaggerOperation(
            Summary = "Lấy ví riêng của cộng tác viên",
            Description = "Lấy thông tin ví của collaborator đang đăng nhập"
        )]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyWallet()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return this.Fail("Token không hợp lệ", "Invalid token", 401);

            var wallet = await _walletService.GetMyCollaboratorWalletAsync(userId);
            return this.Success(wallet, "Lấy thông tin ví thành công", "Get wallet successfully");
        }

        [SwaggerOperation(
            Summary = "Lấy lịch sử giao dịch ví",
            Description = "Lấy danh sách giao dịch của collaborator đang đăng nhập"
        )]
        [HttpGet("my/transactions")]
        public async Task<IActionResult> GetMyTransactions()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return this.Fail("Token không hợp lệ", "Invalid token", 401);

            var transactions = await _walletService.GetMyCollaboratorTransactionsAsync(userId);
            return this.Success(transactions, "Lấy lịch sử giao dịch thành công", "Get transactions successfully");
        }

        [SwaggerOperation(
            Summary = "Thống kê ví",
            Description = "Lấy thống kê số dư, doanh thu, hoa hồng của collaborator"
        )]
        [HttpGet("my/stats")]
        public async Task<IActionResult> GetMyStats()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return this.Fail("Token không hợp lệ", "Invalid token", 401);

            var stats = await _walletService.GetMyCollaboratorStatsAsync(userId);
            return this.Success(stats, "Lấy thống kê ví thành công", "Get wallet stats successfully");
        }
    }
}
