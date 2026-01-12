using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Collaborator;
using DrawingMarketplace.Application.Features.Collaborators;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly DrawingMarketplaceContext _context;

        public CollaboratorController(
            ApplyCollaboratorHandler apply,
            ApproveCollaboratorHandler approve,
            RejectCollaboratorHandler reject,
            GetAllCollaboratorsHandler getAll,
            DrawingMarketplaceContext context)
        {
            _apply = apply;
            _approve = approve;
            _reject = reject;
            _getAll = getAll;
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Apply(
         [FromBody] ApplyCollaboratorRequestDto dto)
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _apply.ExecuteAsync(userId, dto);

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
        [Authorize]
        [HttpGet("collaborator/info")]
        public async Task<IActionResult> GetCollaboratorBasicInfo()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized("Token không hợp lệ");

            var request = await _context.CollaboratorRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (request == null)
                return NotFound("Bạn chưa có đơn đăng ký cộng tác viên");

            Collaborator? collaborator = null;
            if (request.Status == CollaboratorRequestStatus.approved)
            {
                collaborator = await _context.Collaborators
                    .AsNoTracking()
                    .Include(x => x.CollaboratorBanks)
                    .FirstOrDefaultAsync(x => x.UserId == userId);
            }

            var bank = collaborator?.CollaboratorBanks
                .FirstOrDefault(x => x.IsDefault == true)
                ?? collaborator?.CollaboratorBanks.FirstOrDefault();

            var requestDateDisplay = request.CreatedAt.HasValue
                ? (DateTime.UtcNow - request.CreatedAt.Value).Days > 0
                    ? $"{(DateTime.UtcNow - request.CreatedAt.Value).Days} ngày trước"
                    : "Hôm nay"
                : "Chưa xác định";

            var status = request.Status switch
            {
                CollaboratorRequestStatus.pending => new { code = "pending", text = "Chờ duyệt", color = "warning" },
                CollaboratorRequestStatus.approved => new { code = "approved", text = "Đã duyệt", color = "success" },
                CollaboratorRequestStatus.rejected => new { code = "rejected", text = "Bị từ chối", color = "danger" },
                _ => new { code = "unknown", text = "Không xác định", color = "secondary" }
            };

            var info = new
            {
                title = "Thông tin Cộng tác viên",
                subtitle = "Chi tiết đơn đăng ký của bạn",
                status,
                requestDateDisplay,
                commissionRate = collaborator?.CommissionRate,
                commissionRateDisplay = collaborator?.CommissionRate != null
                    ? $"{collaborator.CommissionRate}%"
                    : "Chưa thiết lập",
                bankInfo = bank == null ? null : new
                {
                    bankName = bank.BankName,
                    accountNumber = bank.BankAccount,
                    formatted = $"{bank.BankName}\n{bank.BankAccount}"
                }
            };

            return this.Success(info, "Lấy thông tin cộng tác viên thành công", "Get collaborator info successfully");
        }
        [Authorize]
        [HttpGet("contents")]
        public async Task<IActionResult> GetMyCollaboratorContents()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized("Token không hợp lệ");

            var collaborator = await _context.Collaborators
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (collaborator == null)
                return Forbid();

            var contents = await _context.Contents
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Collaborator)
                    .ThenInclude(c => c.User)
                .Include(x => x.Files)
                .Where(x => x.CollaboratorId == collaborator.Id)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Description,
                    x.Price,
                    x.Status,
                    x.CreatedAt,
                    Category = x.Category,
                    Files = x.Files,
                    Collaborator = x.Collaborator
                })
                .ToListAsync();

            var result = contents.Select(x => new
            {
                id = x.Id,
                title = x.Title,
                description = x.Description,

                category = x.Category == null ? null : new
                {
                    name = x.Category.Name,
                    slug = x.Category.Name
                },

                files = x.Files.Select(f => new
                {
                    name = f.FileName,
                    type = f.FileType,
                    size = f.Size
                }),

                price = x.Price,
                priceDisplay = $"{x.Price:N0} đ",

                status = x.Status == ContentStatus.draft
                    ? new { text = "Chờ duyệt", color = "warning" }
                    : x.Status == ContentStatus.published
                        ? new { text = "Đã duyệt", color = "success" }
                        : x.Status == ContentStatus.archived
                            ? new { text = "Từ chối", color = "danger" }
                            : new { text = "Không xác định", color = "secondary" },

                createdBy = x.Collaborator == null || x.Collaborator.User == null
                    ? null
                    : new
                    {
                        name = x.Collaborator.User.Username,
                        email = x.Collaborator.User.Email
                    },

                createdAt = x.CreatedAt,
                createdAtDisplay = x.CreatedAt.HasValue
                    ? x.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm")
                    : null
            });

            return this.Success(result, "Lấy danh sách nội dung cộng tác viên thành công");
        }
        [Authorize]
        [HttpGet("revenue-stats")]
        public async Task<IActionResult> GetCollaboratorRevenueStats()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            var collaborator = await _context.Collaborators
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (collaborator == null)
                return Forbid();

            var commissionPercent = collaborator.CommissionRate ?? 10m;
            var commissionRate = commissionPercent / 100m;

            var orderItems = await _context.OrderItems
                .AsNoTracking()
                .Include(x => x.Order)
                .Where(x =>
                    x.CollaboratorId == collaborator.Id &&
                    x.Order.Status == OrderStatus.paid)
                .ToListAsync();

            var totalRevenue = orderItems.Sum(x => x.Price);
            var totalOrders = orderItems.Count;
            var commissionAmount = totalRevenue * commissionRate;

            var result = new
            {
                totalRevenue,
                totalRevenueDisplay = $"{totalRevenue:N0} đ",

                commissionAmount,
                commissionAmountDisplay = $"{commissionAmount:N0} đ",

                totalOrders,

                commissionRate = commissionPercent,
                commissionRateDisplay = $"{commissionPercent}%"
            };

            return this.Success(result, "Lấy thống kê doanh thu cộng tác viên thành công");
        }

    }
}
