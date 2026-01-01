using DrawingMarketplace.Application.DTOs.Cart;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;
using MediatR;

namespace DrawingMarketplace.Application.Features.Cart
{
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
            var userId = _currentUser.UserId ?? throw new UnauthorizedException();

            var cart = await _cartRepository.GetByUserIdWithItemsAsync(userId)
                ?? throw new NotFoundException("Cart", userId);

            cart.Clear();

            await _uow.SaveChangesAsync();

            return new CartResponseDto([], 0, 0);
        }

    }

}