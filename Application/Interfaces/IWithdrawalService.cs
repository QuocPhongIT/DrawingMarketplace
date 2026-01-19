using DrawingMarketplace.Application.DTOs.Withdrawal;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IWithdrawalService
    {
        Task<WithdrawalDto> CreateWithdrawalAsync(CreateWithdrawalDto dto);
        Task<WithdrawalDto> ApproveWithdrawalAsync(Guid withdrawalId, Guid adminId);
        Task<WithdrawalDto> RejectWithdrawalAsync(Guid withdrawalId, Guid adminId, string? reason = null);
        Task<WithdrawalDto> MarkAsPaidAsync(Guid withdrawalId, Guid adminId);
        Task<List<WithdrawalDto>> GetCollaboratorWithdrawalsAsync(Guid collaboratorId);
        Task<List<WithdrawalDto>> GetPendingWithdrawalsAsync();
        Task<List<WithdrawalDto>> GetAllWithdrawalsAsync(
            WithdrawalStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 20);
    }
}