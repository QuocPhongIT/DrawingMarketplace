using DrawingMarketplace.Application.DTOs.Collaborator;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace DrawingMarketplace.Application.Features.Collaborators;

public sealed class ApplyCollaboratorHandler
{
    private readonly ICollaboratorRequestRepository _requests;
    private readonly ICollaboratorRepository _collaborators;
    private readonly ICollaboratorBankRepository _banks;
    private readonly IUnitOfWork _uow;

    public ApplyCollaboratorHandler(
        ICollaboratorRequestRepository requests,
        ICollaboratorRepository collaborators,
        ICollaboratorBankRepository banks,
        IUnitOfWork uow)
    {
        _requests = requests;
        _collaborators = collaborators;
        _banks = banks;
        _uow = uow;
    }

    public async Task ExecuteAsync(Guid userId, ApplyCollaboratorRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.BankName))
            throw new ValidationException("Tên ngân hàng không được để trống");
        if (string.IsNullOrWhiteSpace(dto.OwnerName))
            throw new ValidationException("Tên chủ tài khoản không được để trống");
        if (string.IsNullOrWhiteSpace(dto.BankAccount)
            || dto.BankAccount.Length < 6
            || dto.BankAccount.Length > 20
            || !dto.BankAccount.All(char.IsDigit))
            throw new ValidationException("Số tài khoản ngân hàng không hợp lệ");

        if (await _collaborators.ExistsAsync(userId))
            throw new ConflictException("Bạn đã là collaborator rồi.");
        if (await _requests.HasPendingAsync(userId))
            throw new ConflictException("Bạn đã gửi yêu cầu collaborator trước đó.");

        await _uow.BeginTransactionAsync();

        try
        {
            var collaborator = new Collaborator
            {
                UserId = userId,
                Status = CollaboratorActivityStatus.suspended, 
                CommissionRate = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _collaborators.AddAsync(collaborator);
            var bank = new CollaboratorBank
            {
                CollaboratorId = collaborator.Id,
                BankName = dto.BankName.Trim(),
                BankAccount = dto.BankAccount.Trim(),
                OwnerName = dto.OwnerName.Trim().ToUpper(),
                IsDefault = true,
                CreatedAt = DateTime.UtcNow
            };
            await _banks.AddAsync(bank);
            var request = new CollaboratorRequest
            {
                UserId = userId,
                Status = CollaboratorRequestStatus.pending,
                CreatedAt = DateTime.UtcNow
            };
            await _requests.AddAsync(request);
            await _uow.CommitTransactionAsync();
        }
        catch
        {
            await _uow.RollbackTransactionAsync();
            throw;
        }
    }
}
