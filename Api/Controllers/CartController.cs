using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Cart;
using DrawingMarketplace.Application.Features.Cart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly GetCartQueryHandler _get;
        private readonly AddToCartHandler _add;
        private readonly RemoveCartHandler _remove;
        private readonly ClearCartHandler _clear;

        public CartController(
            GetCartQueryHandler get,
            AddToCartHandler add,
            RemoveCartHandler remove,
            ClearCartHandler clear)
        {
            _get = get;
            _add = add;
            _remove = remove;
            _clear = clear;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _get.ExecuteAsync();
            return this.Success(cart, "Lấy giỏ hàng thành công", "Get cart successfully");
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest req)
        {
            var cart = await _add.ExecuteAsync(req.ContentId);
            return this.Success(cart, "Thêm vào giỏ hàng thành công", "Add to cart successfully");
        }

        [HttpDelete("{contentId:guid}")]
        public async Task<IActionResult> Remove(Guid contentId)
        {
            var cart = await _remove.ExecuteAsync(contentId);
            return this.Success(cart, "Xóa khỏi giỏ hàng thành công", "Remove from cart successfully");
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> Clear()
        {
            var cart = await _clear.ExecuteAsync();
            return this.Success(cart, "Xóa toàn bộ giỏ hàng thành công", "Clear cart successfully");
        }
    }
}
