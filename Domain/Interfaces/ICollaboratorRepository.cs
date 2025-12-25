using DrawingMarketplace.Domain.Entities;

namespace DrawingMarketplace.Domain.Interfaces
{
    public interface ICollaboratorRepository
    {
        Task<bool> ExistsAsync(Guid userId);
        Task AddAsync(Collaborator collaborator);
    }
}
