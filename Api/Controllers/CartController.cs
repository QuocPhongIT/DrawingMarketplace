using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Cart;
using DrawingMarketplace.Application.Features.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DrawingMarketplace.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize] 
public class CartController : ControllerBase
{
    private readonly GetCartQueryHandler _getCartHandler;
    private readonly AddToCartHandler _addToCartHandler;
    private readonly RemoveCartHandler _removeCartHandler;
    private readonly ClearCartHandler _clearCartHandler;

    public CartController(
        GetCartQueryHandler getCartHandler,
        AddToCartHandler addToCartHandler,
        RemoveCartHandler removeCartHandler,
        ClearCartHandler clearCartHandler)
    {
        _getCartHandler = getCartHandler;
        _addToCartHandler = addToCartHandler;
        _removeCartHandler = removeCartHandler;
        _clearCartHandler = clearCartHandler;
    }

    [SwaggerOperation(
        Summary = "Lấy giỏ hàng",
        Description = "Lấy danh sách sản phẩm hiện có trong giỏ hàng của người dùng"
    )]
    [Authorize]  
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _getCartHandler.ExecuteAsync();
        return this.Success(cart, "Lấy giỏ hàng thành công", "Get cart successfully");
    }

    [SwaggerOperation(
        Summary = "Thêm vào giỏ hàng",
        Description = "Thêm sản phẩm (content) vào giỏ hàng"
    )]
    [Authorize]  
    [HttpPost]    
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest req)
    {
        var cart = await _addToCartHandler.ExecuteAsync(req);
        return this.Success(cart, "Thêm vào giỏ hàng thành công", "Add to cart successfully");
    }

    [SwaggerOperation(
        Summary = "Xóa sản phẩm khỏi giỏ hàng",
        Description = "Xóa một sản phẩm trong giỏ hàng theo contentId"
    )]
    [Authorize]  
    [HttpDelete("{contentId:guid}")]
    public async Task<IActionResult> Remove(Guid contentId)
    {
        var cart = await _removeCartHandler.ExecuteAsync(contentId);
        return this.Success(cart, "Xóa khỏi giỏ hàng thành công", "Remove from cart successfully");
    }

    [SwaggerOperation(
        Summary = "Xóa toàn bộ giỏ hàng",
        Description = "Xóa tất cả sản phẩm trong giỏ hàng của người dùng"
    )]
    [Authorize] 
    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var cart = await _clearCartHandler.ExecuteAsync();
        return this.Success(cart, "Xóa toàn bộ giỏ hàng thành công", "Clear cart successfully");
    }
}