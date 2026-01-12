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

        public async Task<WalletDto> GetMyCollaboratorWalletAsync(Guid userId)
        {
            var collaborator = await GetCollaboratorByUserIdAsync(userId);
            return await GetCollaboratorWalletAsync(collaborator.Id);
        }

        public async Task<List<WalletTransactionDto>> GetMyCollaboratorTransactionsAsync(Guid userId)
        {
            var collaborator = await GetCollaboratorByUserIdAsync(userId);
            return await GetCollaboratorTransactionsAsync(collaborator.Id);
        }

        public async Task<WalletStatsDto> GetMyCollaboratorStatsAsync(Guid userId)
        {
            var collaborator = await GetCollaboratorByUserIdAsync(userId);
            return await GetCollaboratorStatsAsync(collaborator.Id);
        }

        public async Task<WalletDto> GetCollaboratorWalletAsync(Guid collaboratorId)
        {
            var wallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w =>
                    w.OwnerType == WalletOwnerType.collaborator &&
                    w.OwnerId == collaboratorId)
                ?? throw new NotFoundException("Wallet", collaboratorId);

            return new WalletDto
            {
                Id = wallet.Id,
                Balance = wallet.Balance ?? 0,
                UpdatedAt = wallet.UpdatedAt
            };
        }

        public async Task<List<WalletTransactionDto>> GetCollaboratorTransactionsAsync(Guid collaboratorId)
        {
            var wallet = await GetWalletAsync(WalletOwnerType.collaborator, collaboratorId);

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

        public async Task<WalletStatsDto> GetCollaboratorStatsAsync(Guid collaboratorId)
        {
            var wallet = await _context.Wallets
                .SingleAsync(x =>
                    x.OwnerType == WalletOwnerType.collaborator &&
                    x.OwnerId == collaboratorId
                );

            var totalCommission = await _context.WalletTransactions
                .Where(x => x.WalletId == wallet.Id && x.Type == WalletTxType.commission)
                .SumAsync(x => x.Amount);

            var totalWithdrawn = await _context.WalletTransactions
                .Where(x => x.WalletId == wallet.Id && x.Type == WalletTxType.withdrawal)
                .SumAsync(x => x.Amount);

            var pendingWithdrawal = await _context.Withdrawals
                .Where(x =>
                    x.CollaboratorId == collaboratorId &&
                    x.Status == WithdrawalStatus.pending
                )
                .SumAsync(x => x.Amount);

            return new WalletStatsDto
            {
                TotalBalance = wallet.Balance ?? 0,
                TotalCommission = totalCommission,
                TotalWithdrawn = Math.Abs(totalWithdrawn),
                PendingWithdrawal = pendingWithdrawal
            };
        }


        public async Task CreateCollaboratorWalletAsync(Guid collaboratorId)
        {
            var exists = await _context.Wallets.AnyAsync(w =>
                w.OwnerType == WalletOwnerType.collaborator &&
                w.OwnerId == collaboratorId);

            if (exists)
                return;

            _context.Wallets.Add(new Wallet
            {
                Id = Guid.NewGuid(),
                OwnerType = WalletOwnerType.collaborator,
                OwnerId = collaboratorId,
                Balance = 0,
                UpdatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task AddCommissionAsync(Guid collaboratorId, decimal amount, Guid orderItemId)
        {
            var wallet = await GetWalletAsync(WalletOwnerType.collaborator, collaboratorId);
            var commissionAmount = amount * COMMISSION_RATE;

            wallet.Balance = (wallet.Balance ?? 0) + commissionAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

            _context.WalletTransactions.Add(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Type = WalletTxType.commission,
                Amount = commissionAmount,
                ReferenceId = orderItemId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task RollbackCommissionAsync(Guid collaboratorId, decimal amount, Guid orderItemId)
        {
            var wallet = await GetWalletAsync(WalletOwnerType.collaborator, collaboratorId);
            var commissionAmount = amount * COMMISSION_RATE;

            if ((wallet.Balance ?? 0) < commissionAmount)
                throw new BadRequestException("Số dư ví không đủ");

            wallet.Balance -= commissionAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

            _context.WalletTransactions.Add(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Type = WalletTxType.debit,
                Amount = -commissionAmount,
                ReferenceId = orderItemId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task DeductBalanceAsync(
            WalletOwnerType ownerType,
            Guid ownerId,
            decimal amount,
            WalletTxType txType,
            Guid? referenceId = null)
        {
            var wallet = await GetWalletAsync(ownerType, ownerId);

            if ((wallet.Balance ?? 0) < amount)
                throw new BadRequestException("Số dư ví không đủ");

            wallet.Balance -= amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            _context.WalletTransactions.Add(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Type = txType,
                Amount = amount,
                ReferenceId = referenceId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        private async Task<Wallet> GetWalletAsync(WalletOwnerType ownerType, Guid ownerId)
        {
            return await _context.Wallets
                .FirstOrDefaultAsync(w => w.OwnerType == ownerType && w.OwnerId == ownerId)
                ?? throw new NotFoundException("Wallet", ownerId);
        }

        private async Task<Collaborator> GetCollaboratorByUserIdAsync(Guid userId)
        {
            return await _context.Collaborators
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Collaborator", userId);
        }
    }
}
