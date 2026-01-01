using DrawingMarketplace.Application.DTOs.Wallet;
using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> GetOrCreateWalletAsync(WalletOwnerType ownerType, Guid ownerId);
        Task AddCommissionAsync(Guid collaboratorId, decimal amount, Guid orderItemId);
        Task RollbackCommissionAsync(Guid collaboratorId, decimal amount, Guid orderItemId);
        Task<WalletStatsDto> GetCollaboratorStatsAsync(Guid collaboratorId);
        Task<List<WalletTransactionDto>> GetTransactionsAsync(WalletOwnerType ownerType, Guid ownerId);
        Task DeductBalanceAsync(WalletOwnerType ownerType, Guid ownerId, decimal amount, WalletTxType txType, Guid? referenceId = null);
    }
}

