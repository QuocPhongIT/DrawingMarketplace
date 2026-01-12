using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;

namespace DrawingMarketplace.Application.Features.Collaborators
{
    public sealed class ApproveCollaboratorHandler
    {
        private readonly ICollaboratorRequestRepository _requests;
        private readonly ICollaboratorRepository _collaborators;
        private readonly IUserRoleRepository _userRoles;
        private readonly IWalletService _walletService;
        private readonly IUnitOfWork _uow;

        private static readonly Guid CollaboratorRoleId =
            new("5d8b9e3c-8be4-4815-9fe3-85280f599964");

        public ApproveCollaboratorHandler(
            ICollaboratorRequestRepository requests,
            ICollaboratorRepository collaborators,
            IUserRoleRepository userRoles,
            IWalletService walletService,
            IUnitOfWork uow)
        {
            _requests = requests;
            _collaborators = collaborators;
            _userRoles = userRoles;
            _walletService = walletService;
            _uow = uow;
        }

        public async Task ExecuteAsync(
            Guid requestId,
            Guid adminId,
            CancellationToken ct = default)
        {
            var request = await _requests.GetByIdAsync(requestId)
                ?? throw new NotFoundException("CollaboratorRequest", requestId);

            if (request.Status != CollaboratorRequestStatus.pending)
                throw new ConflictException("Request has already been processed.");

            if (request.UserId == null)
                throw new BadRequestException("Invalid collaborator request.");

            await _uow.BeginTransactionAsync(ct);

            try
            {
                request.Status = CollaboratorRequestStatus.approved;
                request.ApprovedAt = DateTime.UtcNow;
                request.ApprovedBy = adminId;
                await _requests.UpdateAsync(request);

                var collaborator = await _collaborators.GetByUserIdAsync(request.UserId.Value);

                if (collaborator == null)
                {
                    collaborator = new Collaborator
                    {
                        UserId = request.UserId.Value,
                        Status = CollaboratorActivityStatus.approved,
                        CommissionRate = 0,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _collaborators.AddAsync(collaborator);
                    await _uow.SaveChangesAsync(ct);
                }
                else
                {
                    collaborator.Status = CollaboratorActivityStatus.approved;
                    collaborator.UpdatedAt = DateTime.UtcNow;
                }

                await _userRoles.AddRoleAsync(request.UserId.Value, CollaboratorRoleId);

                await _walletService.CreateCollaboratorWalletAsync(collaborator.Id);

                await _uow.CommitTransactionAsync(ct);
            }
            catch
            {
                await _uow.RollbackTransactionAsync(ct);
                throw;
            }
        }
    }
}
