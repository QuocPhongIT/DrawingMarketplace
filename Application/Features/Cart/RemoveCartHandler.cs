using DrawingMarketplace.Application.DTOs.Cart;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Application.Profiles;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;

namespace DrawingMarketplace.Application.Features.Cart;

public sealed class RemoveCartHandler
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;

    public RemoveCartHandler(
        ICartRepository cartRepository,
        ICurrentUserService currentUser,
        IUnitOfWork uow)
    {
        _cartRepository = cartRepository;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task<CartResponseDto> ExecuteAsync(Guid contentId)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User not authenticated");

        var cart = await _cartRepository.GetByUserIdWithItemsAsync(userId);

        if (cart == null || cart.ItemCount == 0)
        {
            return new CartResponseDto([], 0m, 0);
        }

        cart.RemoveItem(contentId);

        _cartRepository.Update(cart);
        await _uow.SaveChangesAsync();

        return CartMapper.Map(cart);
    }
}