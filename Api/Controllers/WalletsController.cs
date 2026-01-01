using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Wallet;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DrawingMarketplace.Infrastructure.Persistence;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/wallets")]
    [Authorize]
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
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var wallet = await _walletService.GetOrCreateWalletAsync(WalletOwnerType.user, userId);
            return this.Success(wallet, "Lấy thông tin ví thành công", "Get wallet successfully");
        }

        [HttpGet("my/transactions")]
        public async Task<IActionResult> GetMyTransactions()
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var transactions = await _walletService.GetTransactionsAsync(WalletOwnerType.user, userId);
            return this.Success(transactions, "Lấy lịch sử giao dịch thành công", "Get transactions successfully");
        }

        [HttpGet("collaborator/stats")]
        [Authorize(Roles = "collaborator")]
        public async Task<IActionResult> GetCollaboratorStats([FromServices] DrawingMarketplace.Infrastructure.Persistence.DrawingMarketplaceContext context)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var collaborator = await context.Collaborators.FirstOrDefaultAsync(c => c.UserId == userId);
            if (collaborator == null)
                return this.NotFound("Collaborator", "Collaborator not found");
            
            var stats = await _walletService.GetCollaboratorStatsAsync(collaborator.Id);
            return this.Success(stats, "Lấy thống kê ví thành công", "Get wallet stats successfully");
        }

        [HttpGet("collaborator/transactions")]
        [Authorize(Roles = "collaborator")]
        public async Task<IActionResult> GetCollaboratorTransactions([FromServices] DrawingMarketplace.Infrastructure.Persistence.DrawingMarketplaceContext context)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var collaborator = await context.Collaborators.FirstOrDefaultAsync(c => c.UserId == userId);
            if (collaborator == null)
                return this.NotFound("Collaborator", "Collaborator not found");
            
            var transactions = await _walletService.GetTransactionsAsync(WalletOwnerType.collaborator, collaborator.Id);
            return this.Success(transactions, "Lấy lịch sử giao dịch thành công", "Get transactions successfully");
        }
    }
}

