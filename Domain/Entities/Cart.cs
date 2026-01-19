using System;
using System.Linq;
using DrawingMarketplace.Domain.Exceptions;

namespace DrawingMarketplace.Domain.Entities;

public partial class Cart
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual User? User { get; set; }

    private Cart() { }

    public static Cart Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new BadRequestException("UserId không hợp lệ");

        return new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void AddItemOrIncrement(Guid contentId, decimal price, int quantity = 1)
    {
        var existing = CartItems.FirstOrDefault(i => i.ContentId == contentId);
        if (existing != null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            var newItem = CartItem.Create(Id, contentId, price, quantity);
            CartItems.Add(newItem);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateItemQuantity(Guid contentId, int newQuantity)
    {
        var item = CartItems.FirstOrDefault(i => i.ContentId == contentId)
            ?? throw new BadRequestException("Không tìm thấy sản phẩm trong giỏ hàng");

        item.UpdateQuantity(newQuantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid contentId)
    {
        var item = CartItems.FirstOrDefault(i => i.ContentId == contentId);
        if (item == null)
            throw new BadRequestException("Không tìm thấy sản phẩm trong giỏ hàng để xóa");

        CartItems.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        CartItems.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal CalculateTotal()
    {
        return CartItems.Sum(i => i.GetSubtotal());  
    }

    public int ItemCount => CartItems.Count; 
}