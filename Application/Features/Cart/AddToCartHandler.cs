using DrawingMarketplace.Application.DTOs.Cart;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Application.Profiles;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;
using MediatR;

namespace DrawingMarketplace.Application.Features.Cart
{
    public sealed class AddToCartHandler
    {
        private readonly ICartRepository _cartRepository;
        private readonly IContentService _contentService;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _uow;

        public AddToCartHandler(
            ICartRepository cartRepository,
            IContentService contentService,
            ICurrentUserService currentUser,
            IUnitOfWork uow)
        {
            _cartRepository = cartRepository;
            _contentService = contentService;
            _currentUser = currentUser;
            _uow = uow;
        }

        public async Task<CartResponseDto> ExecuteAsync(Guid contentId)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedException();

            var content = await _contentService.GetEntityByIdAsync(contentId)
                ?? throw new NotFoundException("Content", contentId);

            if (content.Status != ContentStatus.published)
                throw new BadRequestException("Chỉ có thể thêm content đã được publish vào giỏ hàng");

            var cart = await _cartRepository.GetByUserIdWithItemsAsync(userId);

            if (cart == null)
            {
                cart = Domain.Entities.Cart.Create(userId);
                _cartRepository.Add(cart);
            }

            cart.AddItem(
                CartItem.Create(content.Id, content.Price)
            );

            await _uow.SaveChangesAsync();

            return CartMapper.Map(cart);
        }
    }
}
