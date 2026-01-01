using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly DrawingMarketplaceContext _context;

        public CartRepository(DrawingMarketplaceContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);
        }

        public async Task<Cart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Content)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);
        }

        public void Add(Cart cart)
        {
            _context.Carts.Add(cart);
        }
    }

}
