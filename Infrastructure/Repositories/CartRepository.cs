using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DrawingMarketplace.Infrastructure.Repositories;

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
                    .ThenInclude(c => c.Files)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    public void Add(Cart cart)
    {
        _context.Carts.Add(cart);
    }

    public void Update(Cart cart)
    {
        _context.Carts.Update(cart);
    }

    public void Remove(Cart cart)
    {
        _context.Carts.Remove(cart);
    }

    public async Task RemoveItemAsync(Guid cartId, Guid contentId, CancellationToken ct = default)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ContentId == contentId, ct);

        if (item != null)
        {
            _context.CartItems.Remove(item);
        }
    }

    public async Task<bool> ExistsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Carts
            .AnyAsync(c => c.UserId == userId, ct);
    }
}