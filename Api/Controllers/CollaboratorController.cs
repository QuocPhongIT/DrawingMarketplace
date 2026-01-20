using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Collaborator;
using DrawingMarketplace.Application.Features.Collaborators;
using DrawingMarketplace.Domain.Entities;
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

        [SwaggerOperation(
            Summary = "Đăng ký làm collaborator",
            Description = "User gửi đơn đăng ký trở thành cộng tác viên"
        )]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Apply(
         [FromBody] ApplyCollaboratorRequestDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return this.Fail("Token không hợp lệ", "Invalid token", 401);

            await _apply.ExecuteAsync(userId, dto);

            return this.Success<object>(
                null,
                "Gửi đơn đăng ký collaborator thành công",
                "Apply collaborator successfully",
                202
            );
        }

        [SwaggerOperation(
            Summary = "Lấy danh sách collaborator",
            Description = "Admin xem toàn bộ danh sách collaborator"
        )]
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

        [SwaggerOperation(
            Summary = "Cập nhật trạng thái collaborator",
            Description = "Admin phê duyệt hoặc từ chối đơn đăng ký collaborator"
        )]
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

        [SwaggerOperation(
            Summary = "Lấy thông tin collaborator",
            Description = "Admin hoặc collaborator xem thông tin cơ bản cộng tác viên"
        )]
        [Authorize(Roles = "admin,collaborator")]
        [HttpGet("info")]
        public async Task<IActionResult> GetCollaboratorBasicInfo()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized("Token không hợp lệ");
            if (role == "admin")
            {
                var collaborators = await _context.Collaborators
                    .AsNoTracking()
                    .Include(x => x.User)
                    .Include(x => x.CollaboratorBanks)
                    .Select(x => new
                    {
                        id = x.Id,
                        username = x.User.Username,
                        email = x.User.Email,
                        commissionRate = x.CommissionRate,
                        commissionRateDisplay = x.CommissionRate != null ? $"{x.CommissionRate}%" : "Chưa thiết lập",
                        bank = x.CollaboratorBanks
                            .Where(b => b.IsDefault == true)
                            .Select(b => new
                            {
                                b.Id,
                                b.BankName,
                                b.BankAccount
                            })
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return this.Success(collaborators, "Admin xem danh sách collaborator");
            }
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
                status,
                commissionRate = collaborator?.CommissionRate,
                commissionRateDisplay = collaborator?.CommissionRate != null
                    ? $"{collaborator.CommissionRate}%"
                    : "Chưa thiết lập",
                bankInfo = bank == null ? null : new
                {
                    bank.Id,
                    bank.BankName,
                    bank.BankAccount
                }
            };

            return this.Success(info, "Lấy thông tin cộng tác viên thành công");
        }

        [SwaggerOperation(
            Summary = "Lấy danh sách nội dung collaborator",
            Description = "Admin xem toàn bộ, collaborator chỉ xem nội dung của mình"
        )]
        [Authorize(Roles = "admin,collaborator")]
        [HttpGet("contents")]
        public async Task<IActionResult> GetMyCollaboratorContents()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized("Token không hợp lệ");

            IQueryable<Content> query = _context.Contents
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Collaborator).ThenInclude(c => c.User)
                .Include(x => x.Files);

            if (role == "collaborator")
            {
                var collaborator = await _context.Collaborators
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (collaborator == null)
                    return Forbid();

                query = query.Where(x => x.CollaboratorId == collaborator.Id);
            }
            var contents = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var result = contents.Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                price = x.Price,
                priceDisplay = $"{x.Price:N0} đ",

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

                status = x.Status switch
                {
                    ContentStatus.draft => new { text = "Chờ duyệt", color = "warning" },
                    ContentStatus.published => new { text = "Đã duyệt", color = "success" },
                    ContentStatus.archived => new { text = "Từ chối", color = "danger" },
                    _ => new { text = "Không xác định", color = "secondary" }
                },

                createdBy = x.Collaborator?.User == null ? null : new
                {
                    name = x.Collaborator.User.Username,
                    email = x.Collaborator.User.Email
                },

                createdAt = x.CreatedAt,
                createdAtDisplay = x.CreatedAt?.ToString("dd/MM/yyyy HH:mm")
            });

            return this.Success(result, "Lấy danh sách nội dung cộng tác viên thành công");
        }

        [SwaggerOperation(
            Summary = "Thống kê doanh thu collaborator",
            Description = "Xem thống kê doanh thu và hoa hồng của collaborator"
        )]
        [Authorize(Roles = "admin,collaborator")]
        [HttpGet("revenuestats")]
        public async Task<IActionResult> GetCollaboratorRevenueStats()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            IQueryable<OrderItem> query = _context.OrderItems
                .AsNoTracking()
                .Include(x => x.Order)
                .Where(x => x.Order.Status == OrderStatus.paid);

            decimal commissionPercent = 10m;

            if (role == "collaborator")
            {
                var collaborator = await _context.Collaborators
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (collaborator == null)
                    return Forbid();

                commissionPercent = collaborator.CommissionRate ?? 10m;
                query = query.Where(x => x.CollaboratorId == collaborator.Id);
            }

            var orderItems = await query.ToListAsync();

            var totalRevenue = orderItems.Sum(x => x.Price);
            var commissionAmount = totalRevenue * (commissionPercent / 100m);

            var result = new
            {
                totalRevenue,
                totalRevenueDisplay = $"{totalRevenue:N0} đ",
                commissionAmount,
                commissionAmountDisplay = $"{commissionAmount:N0} đ",
                totalOrders = orderItems.Count,
                commissionRate = commissionPercent,
                commissionRateDisplay = $"{commissionPercent}%"
            };

            return this.Success(result, "Lấy thống kê doanh thu cộng tác viên thành công");
        }

    }
}
