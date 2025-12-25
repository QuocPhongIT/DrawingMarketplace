using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Infrastructure.Repositories
{
    public sealed class CollaboratorRequestRepository : ICollaboratorRequestRepository
    {
        private readonly DrawingMarketplaceContext _db;

        public CollaboratorRequestRepository(DrawingMarketplaceContext db)
        {
            _db = db;
        }

        public async Task<bool> HasPendingAsync(Guid userId)
        {
            return await _db.CollaboratorRequests
                .AnyAsync(r =>
                    r.UserId == userId &&
                    r.Status == CollaboratorRequestStatus.pending);
        }

        public async Task<CollaboratorRequest?> GetByIdAsync(Guid id)
        {
            return await _db.CollaboratorRequests
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(CollaboratorRequest request)
        {
            _db.CollaboratorRequests.Add(request);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(CollaboratorRequest request)
        {
            _db.CollaboratorRequests.Update(request);
            await _db.SaveChangesAsync();
        }
    }
}
