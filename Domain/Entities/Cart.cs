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

    public void AddItem(CartItem item)
    {
        if (item == null)
            throw new BadRequestException("CartItem không được null");

        if (CartItems.Any(i => i.ContentId == item.ContentId))
            throw new BadRequestException("Sản phẩm đã tồn tại trong giỏ hàng");

        CartItems.Add(item);
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
        return CartItems.Sum(i => i.Price);
    }

    public int ItemCount => CartItems.Count;
}