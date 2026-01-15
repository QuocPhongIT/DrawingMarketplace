using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Order;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Runtime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly VnPaySettings _settings;

        public OrdersController(
            IOrderService orderService,
            IPaymentGateway paymentGateway,
            IOptions<VnPaySettings> settings)
        {
            _orderService = orderService;
            _paymentGateway = paymentGateway;
            _settings = settings.Value;
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
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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
        [AllowAnonymous]
        [HttpGet("vnpay/callback")]
        public IActionResult VnPayCallback()
        {
            var query = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            if (!_paymentGateway.VerifySignature(query))
                return Content("<h2>Chữ ký không hợp lệ</h2>", "text/html; charset=utf-8");

            var success = query["vnp_ResponseCode"] == "00";

            return Content($@"
                <html>
                <body style='font-family:Arial;text-align:center;margin-top:50px'>
                    <h2>{(success ? "Thanh toán thành công" : "Thanh toán thất bại")}</h2>
                    <p>Mã giao dịch: {query["vnp_TransactionNo"]}</p>
                    <p>Đơn hàng đang được xử lý</p>
                </body>
                </html>", "text/html; charset=utf-8");
        }
        [AllowAnonymous]
        [HttpGet("vnpay/ipn")]
        public async Task<IActionResult> VnPayIpn()
        {
            var query = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            if (!_paymentGateway.VerifySignature(query))
                return Ok(new { RspCode = "97", Message = "Invalid signature" });

            if (!Guid.TryParse(query["vnp_TxnRef"], out var paymentId))
                return Ok(new { RspCode = "01", Message = "Invalid payment" });

            await _orderService.ProcessPaymentIpnAsync(
                paymentId,
                query["vnp_ResponseCode"],
                query["vnp_TransactionNo"],
                query
            );

            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }
    }
}
