using MediatR;

namespace DrawingMarketplace.Application.DTOs.Cart
{
    public record AddToCartRequest(Guid ContentId, int Quantity = 1):  IRequest<CartResponseDto>;
}
