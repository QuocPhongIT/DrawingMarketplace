using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Order;
using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var order = await _orderService.CreateOrderAsync(dto);
            return this.Success(order, "Tạo đơn hàng thành công", "Create order successfully", 201);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return this.Success(orders, "Lấy danh sách đơn hàng thành công", "Get orders successfully");
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return this.NotFound("Order", "Order not found");

            return this.Success(order, "Lấy chi tiết đơn hàng thành công", "Get order detail successfully");
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var result = await _orderService.CancelOrderAsync(id);
            if (!result)
                return this.NotFound("Order", "Order not found");

            return this.Success<object>(null, "Hủy đơn hàng thành công", "Cancel order successfully");
        }
        [HttpGet("vnpay/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayCallback()
        {
            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());

            if (!queryParams.TryGetValue("vnp_TxnRef", out var txnRef) || !Guid.TryParse(txnRef, out var orderId))
                return BadRequest(new { message = "Thiếu mã đơn hàng" });

            queryParams.TryGetValue("vnp_ResponseCode", out var responseCode);
            queryParams.TryGetValue("vnp_TransactionNo", out var transactionNo);

            string status = responseCode == "00" ? "success" : "failed";

            var order = await _orderService.GetByIdAsync(orderId);
            if (order == null || order.Payment == null)
                return NotFound(new { message = "Order không tồn tại" });

            await _orderService.ProcessPaymentCallbackAsync(order.Payment.Id, status, transactionNo ?? string.Empty);

            return Ok(new { message = "VNPay callback processed", status });
        }
    }
}

