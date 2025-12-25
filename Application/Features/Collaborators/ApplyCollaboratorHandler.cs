using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;

namespace DrawingMarketplace.Application.Features.Collaborators;

public sealed class ApplyCollaboratorHandler
{
    private readonly ICollaboratorRequestRepository _requests;
    private readonly ICollaboratorRepository _collaborators;
    private readonly IUnitOfWork _uow;

    public ApplyCollaboratorHandler(
        ICollaboratorRequestRepository requests,
        ICollaboratorRepository collaborators,
        IUnitOfWork uow)
    {
        _requests = requests;
        _collaborators = collaborators;
        _uow = uow;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        if (await _collaborators.ExistsAsync(userId))
            throw new ConflictException("You are already a collaborator");

        if (await _requests.HasPendingAsync(userId))
            throw new ConflictException("You already have a pending request");

        await _uow.BeginAsync();
        try
        {
            var request = new CollaboratorRequest
            {
                UserId = userId,
                Status = CollaboratorRequestStatus.pending,
                CreatedAt = DateTime.UtcNow
            };

            await _requests.AddAsync(request);

            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }
}
