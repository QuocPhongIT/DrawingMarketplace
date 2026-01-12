using DrawingMarketplace.Domain.Entities;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface ICollaboratorBankRepository
    {
        Task AddAsync(CollaboratorBank bank);

        Task<bool> HasBankAsync(Guid collaboratorId);

        Task<CollaboratorBank?> GetDefaultAsync(Guid collaboratorId);
    }
}
