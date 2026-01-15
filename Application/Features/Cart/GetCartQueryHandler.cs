using DrawingMarketplace.Application.DTOs.Cart;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Application.Profiles;
using DrawingMarketplace.Domain.Exceptions;

namespace DrawingMarketplace.Application.Features.Cart;

public sealed class GetCartQueryHandler
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUserService _currentUser;

    public GetCartQueryHandler(
        ICartRepository cartRepository,
        ICurrentUserService currentUser)
    {
        _cartRepository = cartRepository;
        _currentUser = currentUser;
    }

    public async Task<CartResponseDto> ExecuteAsync()
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User not authenticated");

        var cart = await _cartRepository.GetByUserIdWithItemsAsync(userId, CancellationToken.None);

        if (cart == null || cart.ItemCount == 0)
        {
            return new CartResponseDto(
                Items: Array.Empty<CartItemDto>().AsReadOnly(),
                TotalAmount: 0m,
                ItemCount: 0
            );
        }

        return CartMapper.Map(cart);
    }
}