using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

namespace DrawingMarketplace.Infrastructure.Repositories
{
    public sealed class CollaboratorBankRepository : ICollaboratorBankRepository
    {
        private readonly DrawingMarketplaceContext _context;

        public CollaboratorBankRepository(DrawingMarketplaceContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CollaboratorBank bank)
        {
            await _context.CollaboratorBanks.AddAsync(bank);
        }

        public Task<bool> HasBankAsync(Guid collaboratorId)
        {
            return _context.CollaboratorBanks
                .AnyAsync(b => b.CollaboratorId == collaboratorId);
        }

        public Task<CollaboratorBank?> GetDefaultAsync(Guid collaboratorId)
        {
            return _context.CollaboratorBanks
                .FirstOrDefaultAsync(b =>
                    b.CollaboratorId == collaboratorId &&
                    b.IsDefault == true);

        }
    }
}
