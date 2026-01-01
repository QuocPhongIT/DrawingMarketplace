using System;

namespace DrawingMarketplace.Domain.Entities;

public class CartItem
{
    public Guid CartId { get; private set; }
    public Guid ContentId { get; private set; }
    public decimal Price { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual Cart Cart { get; private set; } = null!;
    public virtual Content Content { get; private set; } = null!;

    private CartItem() { }

    internal static CartItem Create(Guid contentId, decimal price)
    {
        return new CartItem
        {
            ContentId = contentId,
            Price = price,
            CreatedAt = DateTime.UtcNow
        };
    }
}
