using DrawingMarketplace.Application.DTOs.Cart;

namespace DrawingMarketplace.Application.Profiles
{

    public static class CartMapper
    {
        public static CartResponseDto Map(Domain.Entities.Cart cart)
        {
            var items = cart.CartItems.Select(i => new CartItemDto(
                i.ContentId,
                i.Content.Title,
                i.Content.Files.FirstOrDefault()?.FileUrl,
                i.Price
            )).ToList();

            return new CartResponseDto(
                items,
                cart.CalculateTotal(),
                cart.ItemCount
            );
        }
    }


}
