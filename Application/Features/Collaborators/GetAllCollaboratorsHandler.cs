using DrawingMarketplace.Application.DTOs.Collaborator;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Application.Features.Collaborators
{
    public sealed class GetAllCollaboratorsHandler
    {
        private readonly DrawingMarketplaceContext _context;

        public GetAllCollaboratorsHandler(DrawingMarketplaceContext context)
        {
            _context = context;
        }

        public async Task<List<CollaboratorDto>> ExecuteAsync()
        {
            return await _context.Collaborators
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CollaboratorDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Username = c.User!.Username,
                    Email = c.User!.Email,
                    Status = c.Status,
                    CommissionRate = c.CommissionRate,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }
    }
}
