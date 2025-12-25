using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;

namespace DrawingMarketplace.Application.Features.Collaborators;

public sealed class RejectCollaboratorHandler
{
    private readonly ICollaboratorRequestRepository _requests;
    private readonly IUnitOfWork _uow;

    public RejectCollaboratorHandler(
        ICollaboratorRequestRepository requests,
        IUnitOfWork uow)
    {
        _requests = requests;
        _uow = uow;
    }

    public async Task ExecuteAsync(Guid requestId, Guid adminId)
    {
        var request = await _requests.GetByIdAsync(requestId)
            ?? throw new NotFoundException("CollaboratorRequest", requestId);

        if (request.Status != CollaboratorRequestStatus.pending)
            throw new ConflictException("Request has already been processed.");

        await _uow.BeginAsync();
        try
        {
            request.Status = CollaboratorRequestStatus.rejected;
            request.ApprovedAt = DateTime.UtcNow;
            request.ApprovedBy = adminId;

            await _requests.UpdateAsync(request);

            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }
}
