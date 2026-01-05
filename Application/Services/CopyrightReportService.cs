using DrawingMarketplace.Application.DTOs.CopyrightReport;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Application.Services
{
    public sealed class CopyrightReportService
        : ICopyrightReportService
    {
        private readonly DrawingMarketplaceContext _context;
        private readonly ICurrentUserService _currentUser;

        public CopyrightReportService(
            DrawingMarketplaceContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        // ================= USER =================

        public async Task CreateAsync(CreateCopyrightReportRequest request)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var content = await _context.Contents
                .FirstOrDefaultAsync(x => x.Id == request.ContentId)
                ?? throw new NotFoundException("Content", request.ContentId);

            if (content.CreatedBy == userId)
                throw new ForbiddenException("Không thể report sản phẩm của chính bạn");

            var exists = await _context.CopyrightReports
                .AnyAsync(x =>
                    x.ContentId == request.ContentId &&
                    x.ReporterId == userId);

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

        // ================= ADMIN =================

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
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
                return false;

            if (report.Status != ReportStatus.pending)
                throw new BadRequestException("Report đã được xử lý");

            report.Status = ReportStatus.approved;
            report.Content!.Status = ContentStatus.archived;

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> RejectAsync(Guid reportId)
        {
            var report = await _context.CopyrightReports
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
                return false;

            if (report.Status != ReportStatus.pending)
                throw new BadRequestException("Report đã được xử lý");

            report.Status = ReportStatus.rejected;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
