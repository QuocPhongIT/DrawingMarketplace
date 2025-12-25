using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DrawingMarketplace.Infrastructure.Repositories
{
    public sealed class UserRoleRepository : IUserRoleRepository
    {
        private readonly DrawingMarketplaceContext _context;

        public UserRoleRepository(DrawingMarketplaceContext context)
        {
            _context = context;
        }

        public async Task AddRoleAsync(Guid userId, Guid roleId)
        {
            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (exists) return;

            _context.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });
        }
        public async Task<bool> HasRoleAsync(Guid userId, Guid roleId)
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }
    }
}
