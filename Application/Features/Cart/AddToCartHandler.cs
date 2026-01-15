using DrawingMarketplace.Application.DTOs.Cart;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Application.Profiles;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;
using System;

namespace DrawingMarketplace.Application.Features.Cart;

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

    public async Task<CartResponseDto> ExecuteAsync(AddToCartRequest request)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User not authenticated");

        var content = await _contentService.GetEntityByIdAsync(request.ContentId)
            ?? throw new NotFoundException("Content", request.ContentId);

        if (content.Status != ContentStatus.published)
            throw new BadRequestException("Chỉ có thể thêm content đã được publish vào giỏ hàng");

        if (content.Price <= 0)
            throw new BadRequestException("Giá sản phẩm không hợp lệ");

        int quantityToAdd = Math.Max(request.Quantity, 1);

        var cart = await _cartRepository.GetByUserIdWithItemsAsync(userId);

        bool isNewCart = cart == null;

        if (isNewCart)
        {
            cart = Domain.Entities.Cart.Create(userId);
            _cartRepository.Add(cart);
        }
        else
        {
            _cartRepository.Update(cart);
        }

        cart.AddItemOrIncrement(
            contentId: content.Id,
            price: content.Price,
            quantity: quantityToAdd
        );

        await _uow.SaveChangesAsync();

        return CartMapper.Map(cart);
    }
}