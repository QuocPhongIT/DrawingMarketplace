using DrawingMarketplace.Application.DTOs.Cart;
using System.Linq;

namespace DrawingMarketplace.Application.Profiles;

public static class CartMapper
{
    public static CartResponseDto Map(Domain.Entities.Cart cart)
    {
        var items = cart.CartItems.Select(i => new CartItemDto(
            ContentId: i.ContentId,
            Title: i.Content?.Title ?? "Unknown",
            ImageUrl: i.Content?.Files?.FirstOrDefault()?.FileUrl ?? string.Empty,
            Price: i.Price,
            Quantity: i.Quantity,
            Subtotal: i.Price * i.Quantity
        )).ToList();

        return new CartResponseDto(
            Items: items.AsReadOnly(),
            TotalAmount: cart.CalculateTotal(),
            ItemCount: cart.ItemCount
        );
    }
}