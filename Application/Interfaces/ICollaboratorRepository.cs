using DrawingMarketplace.Domain.Entities;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface ICollaboratorRepository
    {
        Task<bool> ExistsAsync(Guid userId);
        Task AddAsync(Collaborator collaborator);
    }
}
