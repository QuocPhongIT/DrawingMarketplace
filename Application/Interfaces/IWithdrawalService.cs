using DrawingMarketplace.Application.DTOs.Withdrawal;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IWithdrawalService
    {
        Task<WithdrawalDto> CreateWithdrawalAsync(CreateWithdrawalDto dto);
        Task<List<WithdrawalDto>> GetCollaboratorWithdrawalsAsync(Guid collaboratorId);
        Task<List<WithdrawalDto>> GetPendingWithdrawalsAsync();
        Task<WithdrawalDto> ApproveWithdrawalAsync(Guid withdrawalId, Guid adminId);
        Task<WithdrawalDto> RejectWithdrawalAsync(Guid withdrawalId, Guid adminId);
    }
}

