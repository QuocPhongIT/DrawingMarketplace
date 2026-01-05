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
        private readonly IUnitOfWork _uow;

        private static readonly Guid CollaboratorRoleId =
            new("5d8b9e3c-8be4-4815-9fe3-85280f599964");

        public ApproveCollaboratorHandler(
            ICollaboratorRequestRepository requests,
            ICollaboratorRepository collaborators,
            IUserRoleRepository userRoles,
            IUnitOfWork uow)
        {
            _requests = requests;
            _collaborators = collaborators;
            _userRoles = userRoles;
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

            if (await _collaborators.ExistsAsync(request.UserId.Value))
                throw new ConflictException("User is already a collaborator.");

            await _uow.BeginTransactionAsync(ct);

            try
            {
                request.Status = CollaboratorRequestStatus.approved;
                request.ApprovedAt = DateTime.UtcNow;
                request.ApprovedBy = adminId;

                await _requests.UpdateAsync(request);

                await _collaborators.AddAsync(new Collaborator
                {
                    UserId = request.UserId.Value,
                    Status = CollaboratorActivityStatus.approved,
                    CommissionRate = 0
                });

                await _userRoles.AddRoleAsync(
                    request.UserId.Value,
                    CollaboratorRoleId);

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