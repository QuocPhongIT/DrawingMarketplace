using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Infrastructure.Repositories
{
    public sealed class CollaboratorRepository : ICollaboratorRepository
    {
        private readonly DrawingMarketplaceContext _db;

        public CollaboratorRepository(DrawingMarketplaceContext db)
        {
            _db = db;
        }

        public async Task<bool> ExistsAsync(Guid userId)
        {
            return await _db.Collaborators
                .AnyAsync(c => c.UserId == userId && c.DeletedAt == null);
        }

        public async Task AddAsync(Collaborator collaborator)
        {
            _db.Collaborators.Add(collaborator);
            await _db.SaveChangesAsync();
        }
    }
}
