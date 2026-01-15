namespace DrawingMarketplace.Application.DTOs.Cart
{
    public record CartItemDto(
     Guid ContentId,
     string Title,
     string ImageUrl,
     decimal Price,
    int Quantity,
    decimal Subtotal);
}
