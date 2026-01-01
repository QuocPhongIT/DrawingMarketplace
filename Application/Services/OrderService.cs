using DrawingMarketplace.Application.DTOs.Order;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly DrawingMarketplaceContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPaymentGatewayService _paymentGatewayService;

        public OrderService(
            DrawingMarketplaceContext context,
            ICurrentUserService currentUserService,
            IPaymentGatewayService paymentGatewayService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _paymentGatewayService = paymentGatewayService;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TotalAmount = dto.TotalAmount,
                    Currency = "VND",
                    Status = OrderStatus.pending,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Orders.Add(order);

                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    UserId = userId,
                    Amount = dto.TotalAmount,
                    PaymentMethod = dto.PaymentMethod,
                    Status = PaymentStatus.pending,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);

                await _context.SaveChangesAsync(); 

                string? paymentUrl = null;

                if (dto.PaymentMethod.Equals("vnpay", StringComparison.OrdinalIgnoreCase))
                {
                    var createPaymentRequest = new CreatePaymentRequest
                    {
                        OrderId = order.Id.ToString(),
                        Amount = payment.Amount,
                        Description = $"Thanh toán đơn hàng #{order.Id}"
                    };

                    var paymentResult = await _paymentGatewayService
                        .GetGateway("vnpay")
                        .CreatePaymentAsync(createPaymentRequest);

                    if (paymentResult.Success && !string.IsNullOrEmpty(paymentResult.PaymentUrl))
                    {
                        paymentUrl = paymentResult.PaymentUrl;
                        var paymentTx = new PaymentTransaction
                        {
                            Id = Guid.NewGuid(),
                            PaymentId = payment.Id,
                            Provider = "vnpay",
                            TransactionId = paymentResult.TransactionId,
                            PaymentUrl = paymentResult.PaymentUrl, 
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.PaymentTransactions.Add(paymentTx);
                    }
                    else
                    {
                        throw new Exception("Không thể tạo link thanh toán VNPAY");
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var orderDto = await MapToOrderDtoAsync(order.Id);

                return orderDto ?? throw new Exception("Không thể lấy thông tin đơn hàng sau khi tạo");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderDto?> GetByIdAsync(Guid id)
        {
            return await MapToOrderDtoAsync(id);
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var list = new List<OrderDto>();
            foreach (var order in orders)
            {
                list.Add(await MapToOrderDtoAsync(order.Id));
            }
            return list;
        }

        public async Task ProcessPaymentCallbackAsync(Guid paymentId, string status, string transactionId)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null || payment.Order == null)
                throw new NotFoundException("Payment", paymentId);

            if (status.ToLower() == "success")
            {
                payment.Status = PaymentStatus.success;
                payment.Order.Status = OrderStatus.paid;
            }
            else
            {
                payment.Status = PaymentStatus.failed;
                payment.Order.Status = OrderStatus.failed;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            order.Status = OrderStatus.cancelled;
            await _context.SaveChangesAsync();
            return true;
        }
        private async Task<OrderDto?> MapToOrderDtoAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Payment)
                    .ThenInclude(p => p.PaymentTransactions)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Content)
                .Include(o => o.OrderCoupon)
                    .ThenInclude(oc => oc.Coupon)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ContentId = oi.ContentId,
                    ContentTitle = oi.Content?.Title ?? "",
                    CollaboratorId = oi.CollaboratorId,
                    Price = oi.Price
                }).ToList(),
                Payment = order.Payment != null ? new PaymentDto
                {
                    Id = order.Payment.Id,
                    OrderId = order.Payment.OrderId,
                    Amount = order.Payment.Amount,
                    PaymentMethod = order.Payment.PaymentMethod,
                    Status = order.Payment.Status,
                    PaymentUrl = order.Payment.PaymentTransactions
                                 .OrderByDescending(t => t.CreatedAt)
                                 .FirstOrDefault()?.PaymentUrl,
                    CreatedAt = order.Payment.CreatedAt
                } : null,
                Coupon = order.OrderCoupon != null ? new CouponDto
                {
                    Code = order.OrderCoupon.Coupon?.Code ?? "",
                    DiscountAmount = order.OrderCoupon.DiscountAmount
                } : null
            };
        }
    }
}