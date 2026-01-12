using DrawingMarketplace.Application.DTOs.CopyrightReport;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Application.Services
{
    public sealed class CopyrightReportService : ICopyrightReportService
    {
        private readonly DrawingMarketplaceContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IEmailService _emailService;

        public CopyrightReportService(
            DrawingMarketplaceContext context,
            ICurrentUserService currentUser,
            IEmailService emailService)
        {
            _context = context;
            _currentUser = currentUser;
            _emailService = emailService;
        }

        public async Task CreateAsync(CreateCopyrightReportRequest request)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var content = await _context.Contents
                .Include(c => c.Collaborator)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(x => x.Id == request.ContentId)
                ?? throw new NotFoundException("Content", request.ContentId);

            if (content.CreatedBy == userId)
                throw new ForbiddenException("Không thể report sản phẩm của chính bạn");

            var exists = await _context.CopyrightReports
                .AnyAsync(x => x.ContentId == request.ContentId && x.ReporterId == userId);

            if (exists)
                throw new BadRequestException("Bạn đã report nội dung này");

            var report = new CopyrightReport
            {
                Id = Guid.NewGuid(),
                ContentId = request.ContentId,
                ReporterId = userId,
                Reason = request.Reason,
                Status = ReportStatus.pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.CopyrightReports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CopyrightReportDto>> GetAllAsync()
        {
            return await _context.CopyrightReports
                .Include(r => r.Content)
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new CopyrightReportDto
                {
                    Id = r.Id,
                    ContentId = r.ContentId,
                    ContentTitle = r.Content!.Title,
                    ReporterId = r.ReporterId,
                    ReporterEmail = r.Reporter!.Email,
                    Reason = r.Reason,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CopyrightReportDto?> GetByIdAsync(Guid id)
        {
            return await _context.CopyrightReports
                .Include(r => r.Content)
                .Include(r => r.Reporter)
                .Where(r => r.Id == id)
                .Select(r => new CopyrightReportDto
                {
                    Id = r.Id,
                    ContentId = r.ContentId,
                    ContentTitle = r.Content!.Title,
                    ReporterId = r.ReporterId,
                    ReporterEmail = r.Reporter!.Email,
                    Reason = r.Reason,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ApproveAsync(Guid reportId)
        {
            var report = await _context.CopyrightReports
                .Include(r => r.Content)
                .ThenInclude(c => c!.Collaborator)
                .ThenInclude(c => c!.User)
                .Include(r => r.Reporter)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
                return false;

            if (report.Status != ReportStatus.pending)
                throw new BadRequestException("Report đã được xử lý");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                report.Status = ReportStatus.approved;
                report.ProcessedAt = DateTime.UtcNow;
                report.ProcessedBy = _currentUser.UserId;

                report.Content!.Status = ContentStatus.archived;
                report.Content.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var reporterEmail = report.Reporter?.Email;
                if (!string.IsNullOrEmpty(reporterEmail))
                {
                    await _emailService.SendCopyrightReportProcessedAsync(
                        reporterEmail,
                        report.Content.Title ?? "Nội dung",
                        "approved",
                        "Báo cáo của bạn đã được chấp nhận. Nội dung vi phạm đã bị khóa và ẩn khỏi hệ thống."
                    );
                }

                var collaboratorEmail = report.Content.Collaborator?.User?.Email;
                if (!string.IsNullOrEmpty(collaboratorEmail))
                {
                    await _emailService.SendCopyrightReportProcessedAsync(
                        collaboratorEmail,
                        report.Content.Title ?? "Nội dung của bạn",
                        "locked",
                        "Nội dung của bạn đã bị khóa và ẩn do vi phạm bản quyền theo báo cáo được chấp nhận."
                    );
                }

                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> RejectAsync(Guid reportId)
        {
            var report = await _context.CopyrightReports
                .Include(r => r.Content)
                .Include(r => r.Reporter)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
                return false;

            if (report.Status != ReportStatus.pending)
                throw new BadRequestException("Report đã được xử lý");

            report.Status = ReportStatus.rejected;
            report.ProcessedAt = DateTime.UtcNow;
            report.ProcessedBy = _currentUser.UserId;

            await _context.SaveChangesAsync();

            var reporterEmail = report.Reporter?.Email;
            if (!string.IsNullOrEmpty(reporterEmail))
            {
                await _emailService.SendCopyrightReportProcessedAsync(
                    reporterEmail,
                    report.Content?.Title ?? "Nội dung",
                    "rejected",
                    "Báo cáo của bạn đã bị từ chối. Nội dung vẫn hoạt động bình thường."
                );
            }

            return true;
        }
    }
}