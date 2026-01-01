using DrawingMarketplace.Domain.Entities;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<Cart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken ct = default);
        void Add(Cart cart);
    }
}
