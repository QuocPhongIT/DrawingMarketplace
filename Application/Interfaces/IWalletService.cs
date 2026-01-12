using DrawingMarketplace.Application.DTOs.Wallet;
using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> GetMyCollaboratorWalletAsync(Guid userId);
        Task<List<WalletTransactionDto>> GetMyCollaboratorTransactionsAsync(Guid userId);
        Task<WalletStatsDto> GetMyCollaboratorStatsAsync(Guid userId);

        Task<WalletDto> GetCollaboratorWalletAsync(Guid collaboratorId);
        Task<List<WalletTransactionDto>> GetCollaboratorTransactionsAsync(Guid collaboratorId);
        Task<WalletStatsDto> GetCollaboratorStatsAsync(Guid collaboratorId);

        Task CreateCollaboratorWalletAsync(Guid collaboratorId);

        Task AddCommissionAsync(Guid collaboratorId, decimal amount, Guid orderItemId);
        Task RollbackCommissionAsync(Guid collaboratorId, decimal amount, Guid orderItemId);

        Task DeductBalanceAsync(
            WalletOwnerType ownerType,
            Guid ownerId,
            decimal amount,
            WalletTxType txType,
            Guid? referenceId = null
        );
    }
}
