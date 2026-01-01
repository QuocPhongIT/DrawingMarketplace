using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Wallet;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    [Authorize(Roles = "collaborator")]
    public class StatisticsController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly DrawingMarketplaceContext _context;

        public StatisticsController(IWalletService walletService, DrawingMarketplaceContext context)
        {
            _walletService = walletService;
            _context = context;
        }

        [HttpGet("wallet")]
        public async Task<IActionResult> GetWalletStats()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var collaborator = await _context.Collaborators.FirstOrDefaultAsync(c => c.UserId == userId);
            if (collaborator == null)
                return this.NotFound("Collaborator", "Collaborator not found");
            
            var stats = await _walletService.GetCollaboratorStatsAsync(collaborator.Id);
            return this.Success(stats, "Lấy thống kê ví thành công", "Get wallet statistics successfully");
        }
    }
}

