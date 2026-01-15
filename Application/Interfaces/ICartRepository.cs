using DrawingMarketplace.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DrawingMarketplace.Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Cart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken ct = default);
    void Add(Cart cart);
    void Update(Cart cart);
    void Remove(Cart cart);
    Task RemoveItemAsync(Guid cartId, Guid contentId, CancellationToken ct = default);
    Task<bool> ExistsForUserAsync(Guid userId, CancellationToken ct = default);
}