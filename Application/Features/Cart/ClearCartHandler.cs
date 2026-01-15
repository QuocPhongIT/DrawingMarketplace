using DrawingMarketplace.Application.DTOs.Cart;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;

namespace DrawingMarketplace.Application.Features.Cart;

public sealed class ClearCartHandler
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;

    public ClearCartHandler(
        ICartRepository cartRepository,
        ICurrentUserService currentUser,
        IUnitOfWork uow)
    {
        _cartRepository = cartRepository;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task<CartResponseDto> ExecuteAsync()
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User not authenticated");

        var cart = await _cartRepository.GetByUserIdWithItemsAsync(userId);

        if (cart == null || cart.ItemCount == 0)
        {
            return new CartResponseDto([], 0m, 0);
        }

        cart.Clear();

        _cartRepository.Update(cart);
        await _uow.SaveChangesAsync();

        return new CartResponseDto([], 0m, 0);
    }
}