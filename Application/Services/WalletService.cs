using DrawingMarketplace.Application.DTOs.Wallet;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly DrawingMarketplaceContext _context;
        private const decimal COMMISSION_RATE = 0.10m;

        public WalletService(DrawingMarketplaceContext context)
        {
            _context = context;
        }

        public async Task<WalletDto> GetOrCreateWalletAsync(WalletOwnerType ownerType, Guid ownerId)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.OwnerType == ownerType && w.OwnerId == ownerId);

            if (wallet == null)
            {
                wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    OwnerType = ownerType,
                    OwnerId = ownerId,
                    Balance = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            return new WalletDto
            {
                Id = wallet.Id,
                Balance = wallet.Balance ?? 0,
                UpdatedAt = wallet.UpdatedAt
            };
        }

        public async Task AddCommissionAsync(Guid collaboratorId, decimal amount, Guid orderItemId)
        {
            var commissionAmount = amount * COMMISSION_RATE;

            var wallet = await GetOrCreateWalletEntityAsync(WalletOwnerType.collaborator, collaboratorId);

            wallet.Balance = (wallet.Balance ?? 0) + commissionAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Type = WalletTxType.commission,
                Amount = commissionAmount,
                ReferenceId = orderItemId,
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task RollbackCommissionAsync(Guid collaboratorId, decimal amount, Guid orderItemId)
        {
            var commissionAmount = amount * COMMISSION_RATE;

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.OwnerType == WalletOwnerType.collaborator && w.OwnerId == collaboratorId);

            if (wallet == null)
                throw new NotFoundException("Wallet", collaboratorId);

            if ((wallet.Balance ?? 0) < commissionAmount)
                throw new BadRequestException("Số dư ví không đủ để rollback hoa hồng");

            wallet.Balance = (wallet.Balance ?? 0) - commissionAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Type = WalletTxType.debit,
                Amount = -commissionAmount,
                ReferenceId = orderItemId,
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeductBalanceAsync(WalletOwnerType ownerType, Guid ownerId, decimal amount, WalletTxType txType, Guid? referenceId = null)
        {
            var wallet = await GetOrCreateWalletEntityAsync(ownerType, ownerId);

            if ((wallet.Balance ?? 0) < amount)
                throw new BadRequestException("Số dư ví không đủ");

            wallet.Balance = (wallet.Balance ?? 0) - amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Type = txType,
                Amount = amount,
                ReferenceId = referenceId,
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<WalletStatsDto> GetCollaboratorStatsAsync(Guid collaboratorId)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.OwnerType == WalletOwnerType.collaborator && w.OwnerId == collaboratorId);

            if (wallet == null)
            {
                return new WalletStatsDto
                {
                    TotalBalance = 0,
                    TotalCommission = 0,
                    TotalWithdrawn = 0,
                    PendingWithdrawal = 0
                };
            }

            var totalCommission = await _context.WalletTransactions
                .Where(t => t.WalletId == wallet.Id && t.Type == WalletTxType.commission)
                .SumAsync(t => t.Amount);

            var totalWithdrawn = await _context.WalletTransactions
                .Where(t => t.WalletId == wallet.Id && t.Type == WalletTxType.withdrawal)
                .SumAsync(t => t.Amount);

            var pendingWithdrawal = await _context.Withdrawals
                .Where(w => w.CollaboratorId == collaboratorId && w.Status == WithdrawalStatus.pending)
                .SumAsync(w => w.Amount);

            return new WalletStatsDto
            {
                TotalBalance = wallet.Balance ?? 0,
                TotalCommission = totalCommission,
                TotalWithdrawn = Math.Abs(totalWithdrawn),
                PendingWithdrawal = pendingWithdrawal
            };
        }

        public async Task<List<WalletTransactionDto>> GetTransactionsAsync(WalletOwnerType ownerType, Guid ownerId)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.OwnerType == ownerType && w.OwnerId == ownerId);

            if (wallet == null)
                return new List<WalletTransactionDto>();

            return await _context.WalletTransactions
                .Where(t => t.WalletId == wallet.Id)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    Type = t.Type.ToString(),
                    Amount = t.Amount,
                    ReferenceId = t.ReferenceId,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
        }

        private async Task<Wallet> GetOrCreateWalletEntityAsync(WalletOwnerType ownerType, Guid ownerId)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.OwnerType == ownerType && w.OwnerId == ownerId);

            if (wallet == null)
            {
                wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    OwnerType = ownerType,
                    OwnerId = ownerId,
                    Balance = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            return wallet;
        }
    }
}

