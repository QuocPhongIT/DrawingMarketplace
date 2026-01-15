using System;
using DrawingMarketplace.Domain.Exceptions;

namespace DrawingMarketplace.Domain.Entities;

public class CartItem
{
    public Guid CartId { get; private set; }
    public Guid ContentId { get; private set; }
    public int Quantity { get; private set; } = 1;
    public decimal Price { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public virtual Cart Cart { get; private set; } = null!;
    public virtual Content Content { get; private set; } = null!;

    private CartItem() { }

    public static CartItem Create(Guid cartId, Guid contentId, decimal price, int quantity = 1)
    {
        if (cartId == Guid.Empty) throw new BadRequestException("CartId không hợp lệ");
        if (contentId == Guid.Empty) throw new BadRequestException("ContentId không hợp lệ");
        if (price < 0) throw new BadRequestException("Giá không được âm");
        if (quantity <= 0) throw new BadRequestException("Số lượng phải lớn hơn 0");

        return new CartItem
        {
            CartId = cartId,
            ContentId = contentId,
            Price = price,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void IncreaseQuantity(int additionalQuantity)
    {
        if (additionalQuantity <= 0) throw new BadRequestException("Số lượng tăng phải lớn hơn 0");
        Quantity += additionalQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0) throw new BadRequestException("Số lượng phải lớn hơn 0");
        if (newQuantity == Quantity) return;
        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetSubtotal() => Price * Quantity;
}