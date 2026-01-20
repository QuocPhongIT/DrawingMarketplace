using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Order;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
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

        [SwaggerOperation(
            Summary = "Tạo đơn hàng",
            Description = "Tạo đơn hàng mới từ danh sách sản phẩm người dùng chọn"
        )]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var order = await _orderService.CreateOrderAsync(dto);
            return this.Success(order, "Tạo đơn hàng thành công", "Create order successfully", 201);
        }

        [SwaggerOperation(
            Summary = "Tạo đơn hàng",
            Description = "Tạo đơn hàng mới từ danh sách sản phẩm người dùng chọn"
        )]
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return this.Fail("Token không hợp lệ", "Invalid token", 401);

            var orders = await _orderService.GetUserOrdersAsync(userId);
            return this.Success(orders, "Lấy danh sách đơn hàng thành công", "Get orders successfully");
        }

        [SwaggerOperation(
            Summary = "Lấy chi tiết đơn hàng",
            Description = "Lấy thông tin chi tiết đơn hàng theo Id"
        )]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return this.NotFound("Order", "Order not found");

            return this.Success(order, "Lấy chi tiết đơn hàng thành công", "Get order detail successfully");
        }

        [SwaggerOperation(
            Summary = "Hủy đơn hàng",
            Description = "Hủy đơn hàng khi đơn chưa được xử lý"
        )]
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var result = await _orderService.CancelOrderAsync(id);
            if (!result)
                return this.NotFound("Order", "Order not found");

            return this.Success<object>(null, "Hủy đơn hàng thành công", "Cancel order successfully");
        }

        [SwaggerOperation(
            Summary = "VNPay callback",
            Description = "Trang hiển thị kết quả thanh toán sau khi VNPay redirect"
        )]
        [AllowAnonymous]
        [HttpGet("vnpay/callback")]
        public IActionResult VnPayCallback()
        {
            var query = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            if (!_paymentGateway.VerifySignature(query))
                return this.Fail(
                    "Chữ ký không hợp lệ",
                    "Invalid signature",
                    400
                );

            var success = query["vnp_ResponseCode"] == "00";

            return this.Success(
                new
                {
                    orderId = query["vnp_TxnRef"],
                    transactionId = query["vnp_TransactionNo"],
                    responseCode = query["vnp_ResponseCode"]
                },
                success ? "Thanh toán thành công" : "Thanh toán thất bại",
                success ? "Payment successful" : "Payment failed"
            );
        }

        [SwaggerOperation(
            Summary = "VNPay IPN",
            Description = "Endpoint VNPay gọi server-to-server để xác nhận trạng thái thanh toán"
        )]
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
