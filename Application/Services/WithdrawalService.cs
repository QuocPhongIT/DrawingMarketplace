using DrawingMarketplace.Application.DTOs.Withdrawal;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Application.Services
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly DrawingMarketplaceContext _context;
        private readonly IWalletService _walletService;
        private const decimal MIN_WITHDRAWAL_AMOUNT = 500000m; 
        private const decimal TAX_THRESHOLD = 2000000m;
        private const decimal TAX_RATE = 0.10m;
        private const decimal TRANSFER_FEE = 11000m;

        public WithdrawalService(
            DrawingMarketplaceContext context,
            IWalletService walletService)
        {
            _context = context;
            _walletService = walletService;
        }

        public async Task<WithdrawalDto> CreateWithdrawalAsync(CreateWithdrawalDto dto)
        {
            if (dto.Amount < MIN_WITHDRAWAL_AMOUNT)
                throw new BadRequestException($"Số tiền rút tối thiểu là {MIN_WITHDRAWAL_AMOUNT:N0} VNĐ");

            // Get collaborator from bank
            var bank = await _context.CollaboratorBanks
                .Include(b => b.Collaborator)
                .FirstOrDefaultAsync(b => b.Id == dto.BankId);

            if (bank == null || bank.Collaborator == null)
                throw new NotFoundException("Bank account", dto.BankId);

            var collaboratorId = bank.CollaboratorId ?? throw new BadRequestException("Bank account không hợp lệ");

            // Check wallet balance
            var wallet = await _walletService.GetOrCreateWalletAsync(WalletOwnerType.collaborator, collaboratorId);
            var availableBalance = wallet.Balance;

            if (availableBalance < dto.Amount)
                throw new BadRequestException("Số dư ví không đủ");

            // Calculate fees and tax
            decimal taxAmount = 0;
            if (dto.Amount >= TAX_THRESHOLD)
            {
                taxAmount = dto.Amount * TAX_RATE;
            }

            decimal feeAmount = TRANSFER_FEE;
            decimal finalAmount = dto.Amount - taxAmount - feeAmount;

            if (availableBalance < dto.Amount)
                throw new BadRequestException("Số dư ví không đủ để chi trả phí và thuế");

            // Create withdrawal
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

            return await MapToDtoAsync(withdrawal.Id);
        }

        public async Task<WithdrawalDto> ApproveWithdrawalAsync(Guid withdrawalId, Guid adminId)
        {
            var withdrawal = await _context.Withdrawals
                .Include(w => w.Bank)
                .Include(w => w.Collaborator)
                .FirstOrDefaultAsync(w => w.Id == withdrawalId);

            if (withdrawal == null)
                throw new NotFoundException("Withdrawal", withdrawalId);

            if (withdrawal.Status != WithdrawalStatus.pending)
                throw new BadRequestException("Yêu cầu rút tiền đã được xử lý");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Calculate tax and fee
                decimal taxAmount = 0;
                if (withdrawal.Amount >= TAX_THRESHOLD)
                {
                    taxAmount = withdrawal.Amount * TAX_RATE;
                }

                decimal feeAmount = TRANSFER_FEE;
                decimal totalDeduct = withdrawal.Amount; // Deduct full amount from wallet

                // Deduct from wallet
                await _walletService.DeductBalanceAsync(
                    WalletOwnerType.collaborator,
                    withdrawal.CollaboratorId!.Value,
                    totalDeduct,
                    WalletTxType.withdrawal,
                    withdrawal.Id
                );

                // Update withdrawal status
                withdrawal.Status = WithdrawalStatus.approved;
                withdrawal.ProcessedAt = DateTime.UtcNow;
                withdrawal.ProcessedBy = adminId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await MapToDtoAsync(withdrawal.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<WithdrawalDto> RejectWithdrawalAsync(Guid withdrawalId, Guid adminId)
        {
            var withdrawal = await _context.Withdrawals
                .FirstOrDefaultAsync(w => w.Id == withdrawalId);

            if (withdrawal == null)
                throw new NotFoundException("Withdrawal", withdrawalId);

            if (withdrawal.Status != WithdrawalStatus.pending)
                throw new BadRequestException("Yêu cầu rút tiền đã được xử lý");

            withdrawal.Status = WithdrawalStatus.rejected;
            withdrawal.ProcessedAt = DateTime.UtcNow;
            withdrawal.ProcessedBy = adminId;

            await _context.SaveChangesAsync();

            return await MapToDtoAsync(withdrawal.Id);
        }

        public async Task<List<WithdrawalDto>> GetCollaboratorWithdrawalsAsync(Guid collaboratorId)
        {
            var withdrawalIds = await _context.Withdrawals
                .Where(w => w.CollaboratorId == collaboratorId)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => w.Id)
                .ToListAsync();

            var withdrawals = new List<WithdrawalDto>();
            foreach (var id in withdrawalIds)
            {
                var dto = await MapToDtoAsync(id);
                withdrawals.Add(dto);
            }
            return withdrawals;
        }

        public async Task<List<WithdrawalDto>> GetPendingWithdrawalsAsync()
        {
            var withdrawalIds = await _context.Withdrawals
                .Where(w => w.Status == WithdrawalStatus.pending)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => w.Id)
                .ToListAsync();

            var withdrawals = new List<WithdrawalDto>();
            foreach (var id in withdrawalIds)
            {
                var dto = await MapToDtoAsync(id);
                withdrawals.Add(dto);
            }
            return withdrawals;
        }

        private async Task<WithdrawalDto> MapToDtoAsync(Guid withdrawalId)
        {
            var withdrawal = await _context.Withdrawals
                .Include(w => w.Bank)
                .FirstOrDefaultAsync(w => w.Id == withdrawalId);

            if (withdrawal == null)
                throw new NotFoundException("Withdrawal", withdrawalId);

            decimal taxAmount = 0;
            if (withdrawal.Amount >= TAX_THRESHOLD)
            {
                taxAmount = withdrawal.Amount * TAX_RATE;
            }

            decimal feeAmount = TRANSFER_FEE;
            decimal finalAmount = withdrawal.Amount - taxAmount - feeAmount;

            return new WithdrawalDto
            {
                Id = withdrawal.Id,
                CollaboratorId = withdrawal.CollaboratorId ?? Guid.Empty,
                BankId = withdrawal.BankId ?? Guid.Empty,
                BankName = withdrawal.Bank?.BankName ?? "",
                BankAccount = withdrawal.Bank?.BankAccount ?? "",
                OwnerName = withdrawal.Bank?.OwnerName ?? "",
                Amount = withdrawal.Amount,
                TaxAmount = taxAmount,
                FeeAmount = feeAmount,
                FinalAmount = finalAmount,
                Status = withdrawal.Status,
                CreatedAt = withdrawal.CreatedAt,
                ProcessedAt = withdrawal.ProcessedAt
            };
        }
    }
}

