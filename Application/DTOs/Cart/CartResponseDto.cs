namespace DrawingMarketplace.Application.DTOs.Cart
{
    public record CartResponseDto(
    IReadOnlyList<CartItemDto> Items,
    decimal TotalAmount,
    int ItemCount);
}
