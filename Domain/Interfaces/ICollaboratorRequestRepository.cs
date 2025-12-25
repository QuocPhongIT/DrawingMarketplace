using DrawingMarketplace.Domain.Entities;

namespace DrawingMarketplace.Domain.Interfaces
{
    public interface ICollaboratorRequestRepository
    {
        Task<bool> HasPendingAsync(Guid userId);
        Task<CollaboratorRequest?> GetByIdAsync(Guid id);
        Task AddAsync(CollaboratorRequest request);
        Task UpdateAsync(CollaboratorRequest request);
    }
}
