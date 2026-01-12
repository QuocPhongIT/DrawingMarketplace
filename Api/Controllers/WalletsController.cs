using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("my")]
        public async Task<IActionResult> GetMyWallet()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var wallet = await _walletService.GetMyCollaboratorWalletAsync(userId);
            return this.Success(wallet, "Lấy thông tin ví thành công", "Get wallet successfully");
        }

        [HttpGet("my/transactions")]
        public async Task<IActionResult> GetMyTransactions()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var transactions = await _walletService.GetMyCollaboratorTransactionsAsync(userId);
            return this.Success(transactions, "Lấy lịch sử giao dịch thành công", "Get transactions successfully");
        }

        [HttpGet("my/stats")]
        public async Task<IActionResult> GetMyStats()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var stats = await _walletService.GetMyCollaboratorStatsAsync(userId);
            return this.Success(stats, "Lấy thống kê ví thành công", "Get wallet stats successfully");
        }
    }
}
