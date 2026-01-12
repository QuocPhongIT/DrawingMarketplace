using DrawingMarketplace.Application.DTOs.Withdrawal;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Application.Services
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly DrawingMarketplaceContext _context;
        private readonly IWalletService _walletService;
        private readonly IEmailService _emailService;

        private const decimal MIN_WITHDRAWAL_AMOUNT = 5000m;
        private const decimal TAX_THRESHOLD = 2000000m;
        private const decimal TAX_RATE = 0.10m;
        private const decimal TRANSFER_FEE = 11000m;
        private const decimal FEE_THRESHOLD = 500000m;

        public WithdrawalService(
            DrawingMarketplaceContext context,
            IWalletService walletService,
            IEmailService emailService)
        {
            _context = context;
            _walletService = walletService;
            _emailService = emailService;
        }

        private decimal CalculateTransferFee(decimal amount)
        {
            return amount >= FEE_THRESHOLD ? TRANSFER_FEE : 0m;
        }

        public async Task<WithdrawalDto> CreateWithdrawalAsync(CreateWithdrawalDto dto)
        {
            if (dto.Amount < MIN_WITHDRAWAL_AMOUNT)
                throw new BadRequestException($"Số tiền rút tối thiểu là {MIN_WITHDRAWAL_AMOUNT:N0} VNĐ");

            var bank = await _context.CollaboratorBanks
                .Include(b => b.Collaborator)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(b => b.Id == dto.BankId);

            if (bank == null || bank.CollaboratorId == null)
                throw new NotFoundException("Bank account", dto.BankId);

            var collaboratorId = bank.CollaboratorId.Value;
            var collaboratorEmail = bank.Collaborator?.User?.Email
                ?? throw new InvalidOperationException("Không tìm thấy email collaborator");

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w =>
                    w.OwnerType == WalletOwnerType.collaborator &&
                    w.OwnerId == collaboratorId);

            if (wallet == null)
                throw new NotFoundException("Wallet", collaboratorId);

            if (wallet.Balance.GetValueOrDefault() < dto.Amount)
                throw new BadRequestException("Số dư ví không đủ");

            var tax = dto.Amount >= TAX_THRESHOLD ? dto.Amount * TAX_RATE : 0m;
            var fee = CalculateTransferFee(dto.Amount);
            var finalAmount = dto.Amount - tax - fee;

            if (finalAmount < 0)
            {
                throw new BadRequestException(
                    $"Số tiền rút không đủ để trừ phí và thuế (phí dự kiến: {fee:N0} VNĐ, thuế: {tax:N0} VNĐ)");
            }

            var withdrawal = new Withdrawal
            {
                Id = Guid.NewGuid(),
                CollaboratorId = collaboratorId,
                BankId = dto.BankId,
                Amount = dto.Amount,
                Status = WithdrawalStatus.pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Withdrawals.Add(withdrawal);
            await _context.SaveChangesAsync();

            await _emailService.SendWithdrawalCreatedAsync(collaboratorEmail, dto.Amount);

            return await MapToDtoAsync(withdrawal.Id);
        }

        public async Task<WithdrawalDto> ApproveWithdrawalAsync(Guid withdrawalId, Guid adminId)
        {
            var withdrawal = await _context.Withdrawals
                .Include(w => w.Collaborator)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(w => w.Id == withdrawalId);

            if (withdrawal == null)
                throw new NotFoundException("Withdrawal", withdrawalId);

            if (withdrawal.Status != WithdrawalStatus.pending)
                throw new BadRequestException("Yêu cầu rút tiền đã được xử lý");

            var collaboratorEmail = withdrawal.Collaborator?.User?.Email
                ?? throw new InvalidOperationException("Không tìm thấy email collaborator");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _walletService.DeductBalanceAsync(
                    WalletOwnerType.collaborator,
                    withdrawal.CollaboratorId!.Value,
                    withdrawal.Amount,
                    WalletTxType.withdrawal,
                    withdrawal.Id
                );

                withdrawal.Status = WithdrawalStatus.approved;
                withdrawal.ProcessedAt = DateTime.UtcNow;
                withdrawal.ProcessedBy = adminId;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var estimatedFinal = withdrawal.Amount - CalculateTransferFee(withdrawal.Amount);
                await _emailService.SendWithdrawalApprovedAsync(collaboratorEmail, withdrawal.Amount, estimatedFinal);

                return await MapToDtoAsync(withdrawal.Id);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<WithdrawalDto> RejectWithdrawalAsync(Guid withdrawalId, Guid adminId, string? reason = null)
        {
            var withdrawal = await _context.Withdrawals
                .Include(w => w.Collaborator)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(w => w.Id == withdrawalId);

            if (withdrawal == null)
                throw new NotFoundException("Withdrawal", withdrawalId);

            if (withdrawal.Status != WithdrawalStatus.pending)
                throw new BadRequestException("Yêu cầu rút tiền đã được xử lý");

            var collaboratorEmail = withdrawal.Collaborator?.User?.Email
                ?? throw new InvalidOperationException("Không tìm thấy email collaborator");

            withdrawal.Status = WithdrawalStatus.rejected;
            withdrawal.ProcessedAt = DateTime.UtcNow;
            withdrawal.ProcessedBy = adminId;

            await _context.SaveChangesAsync();

            await _emailService.SendWithdrawalRejectedAsync(collaboratorEmail, withdrawal.Amount, reason);

            return await MapToDtoAsync(withdrawal.Id);
        }

        public async Task<WithdrawalDto> MarkAsPaidAsync(Guid withdrawalId, Guid adminId)
        {
            var withdrawal = await _context.Withdrawals
                .Include(w => w.Collaborator)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(w => w.Id == withdrawalId);

            if (withdrawal == null)
                throw new NotFoundException("Withdrawal", withdrawalId);

            if (withdrawal.Status != WithdrawalStatus.approved)
                throw new BadRequestException("Chỉ có thể đánh dấu 'paid' khi yêu cầu đã được duyệt");

            var collaboratorEmail = withdrawal.Collaborator?.User?.Email
                ?? throw new InvalidOperationException("Không tìm thấy email collaborator");

            withdrawal.Status = WithdrawalStatus.paid;
            withdrawal.ProcessedAt = DateTime.UtcNow;
            withdrawal.ProcessedBy = adminId;

            await _context.SaveChangesAsync();

            await _emailService.SendWithdrawalPaidAsync(collaboratorEmail, withdrawal.Amount);

            return await MapToDtoAsync(withdrawal.Id);
        }

        public async Task<List<WithdrawalDto>> GetCollaboratorWithdrawalsAsync(Guid collaboratorId)
        {
            var ids = await _context.Withdrawals
                .Where(w => w.CollaboratorId == collaboratorId)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => w.Id)
                .ToListAsync();

            var result = new List<WithdrawalDto>();
            foreach (var id in ids)
            {
                result.Add(await MapToDtoAsync(id));
            }

            return result;
        }

        public async Task<List<WithdrawalDto>> GetPendingWithdrawalsAsync()
        {
            var ids = await _context.Withdrawals
                .Where(w => w.Status == WithdrawalStatus.pending)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => w.Id)
                .ToListAsync();

            var result = new List<WithdrawalDto>();
            foreach (var id in ids)
            {
                result.Add(await MapToDtoAsync(id));
            }

            return result;
        }

        public async Task<List<WithdrawalDto>> GetAllWithdrawalsAsync(
            WithdrawalStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Withdrawals.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(w => w.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(w => w.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(w => w.CreatedAt <= toDate.Value);
            }

            var ids = await query
                .OrderByDescending(w => w.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => w.Id)
                .ToListAsync();

            var result = new List<WithdrawalDto>();
            foreach (var id in ids)
            {
                result.Add(await MapToDtoAsync(id));
            }

            return result;
        }

        private async Task<WithdrawalDto> MapToDtoAsync(Guid withdrawalId)
        {
            var withdrawal = await _context.Withdrawals
                .Include(w => w.Bank)
                .FirstOrDefaultAsync(w => w.Id == withdrawalId);

            if (withdrawal == null)
                throw new NotFoundException("Withdrawal", withdrawalId);

            var tax = withdrawal.Amount >= TAX_THRESHOLD
                ? withdrawal.Amount * TAX_RATE
                : 0m;

            var fee = CalculateTransferFee(withdrawal.Amount);

            return new WithdrawalDto
            {
                Id = withdrawal.Id,
                CollaboratorId = withdrawal.CollaboratorId ?? Guid.Empty,
                BankId = withdrawal.BankId ?? Guid.Empty,
                BankName = withdrawal.Bank?.BankName ?? "",
                BankAccount = withdrawal.Bank?.BankAccount ?? "",
                OwnerName = withdrawal.Bank?.OwnerName ?? "",
                Amount = withdrawal.Amount,
                TaxAmount = tax,
                FeeAmount = fee,
                FinalAmount = withdrawal.Amount - tax - fee,
                Status = withdrawal.Status,
                CreatedAt = withdrawal.CreatedAt,
                ProcessedAt = withdrawal.ProcessedAt
            };
        }
    }
}