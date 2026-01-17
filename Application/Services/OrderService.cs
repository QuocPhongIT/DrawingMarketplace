using DrawingMarketplace.Application.DTOs.Order;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DrawingMarketplace.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly DrawingMarketplaceContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly ILogger<OrderService> _logger;
        public OrderService(
            DrawingMarketplaceContext context,
            ICurrentUserService currentUserService,
            IPaymentGatewayService paymentGatewayService,
            ILogger<OrderService> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _paymentGatewayService = paymentGatewayService;
            _logger = logger;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

            using var tx = await _context.Database.BeginTransactionAsync();

            List<(Guid ContentId, int Quantity, decimal Price, Guid? CollaboratorId)> items;

            if (dto.ContentId.HasValue)
            {
                var content = await _context.Contents.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.ContentId.Value)
                    ?? throw new NotFoundException("Content", dto.ContentId.Value);

                items = new() { (content.Id, dto.Quantity, content.Price, content.CollaboratorId) };
            }
            else
            {
                var cart = await _context.Carts
                    .Include(x => x.CartItems)
                    .FirstOrDefaultAsync(x => x.UserId == userId)
                    ?? throw new NotFoundException("Cart", userId);

                if (!cart.CartItems.Any())
                    throw new Exception("Giỏ hàng trống");

                var ids = cart.CartItems.Select(x => x.ContentId).Distinct().ToList();

                var contents = await _context.Contents.AsNoTracking()
                    .Where(x => ids.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id);

                items = cart.CartItems.Select(x => (
                    x.ContentId,
                    x.Quantity,
                    contents[x.ContentId].Price,
                    contents[x.ContentId].CollaboratorId
                )).ToList();

                _context.CartItems.RemoveRange(cart.CartItems);
                _context.Carts.Remove(cart);
            }

            var subtotal = items.Sum(x => x.Price * x.Quantity);

            decimal discount = 0;
            Coupon? coupon = null;

            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                coupon = await _context.Coupons
                     .FirstOrDefaultAsync(x =>
                         x.Code == dto.CouponCode &&
                         x.IsActive == true
                     );


                if (coupon != null)
                {
                    discount = coupon.Type == CouponType.percent
                        ? subtotal * coupon.Value / 100
                        : coupon.Value;

                    if (coupon.MaxDiscount.HasValue)
                        discount = Math.Min(discount, coupon.MaxDiscount.Value);
                }
            }

            var total = Math.Max(0, subtotal - discount);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TotalAmount = total,
                Currency = "VND",
                Status = OrderStatus.pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);

            foreach (var i in items)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ContentId = i.ContentId,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    CollaboratorId = i.CollaboratorId
                });
            }

            if (coupon != null && discount > 0)
            {
                order.OrderCoupon = new OrderCoupon
                {
                    CouponId = coupon.Id,
                    DiscountAmount = discount,
                    AppliedAt = DateTime.UtcNow
                };
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                UserId = userId,
                Amount = total,
                PaymentMethod = dto.PaymentMethod,
                Status = PaymentStatus.pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            if (dto.PaymentMethod.Equals("vnpay", StringComparison.OrdinalIgnoreCase))
            {
                var gateway = _paymentGatewayService.GetGateway("vnpay");

                var result = await gateway.CreatePaymentAsync(new CreatePaymentRequest
                {
                    OrderId = payment.Id.ToString(),
                    Amount = total,
                    Description = $"Thanh toán đơn hàng {order.Id}"
                });

                if (!result.Success || string.IsNullOrEmpty(result.PaymentUrl))
                    throw new Exception("Không thể tạo link thanh toán");

                _context.PaymentTransactions.Add(new PaymentTransaction
                {
                    Id = Guid.NewGuid(),
                    PaymentId = payment.Id,
                    Provider = "vnpay",
                    TransactionId = result.TransactionId,
                    PaymentUrl = result.PaymentUrl,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return await MapToOrderDtoAsync(order.Id)
                ?? throw new Exception("Map OrderDto failed");
        }

        public async Task ProcessPaymentIpnAsync(
      Guid paymentId,
      string responseCode,
      string transactionNo,
      Dictionary<string, string> rawData)
        {
            var payment = await _context.Payments
                .Include(x => x.Order)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == paymentId);

            if (payment == null)
            {
                _logger.LogWarning("IPN: Payment not found {PaymentId}", paymentId);
                return;
            }

            if (payment.Order == null)
            {
                _logger.LogWarning("IPN: Order not found for payment {PaymentId}", paymentId);
                return;
            }

            if (payment.Status == PaymentStatus.success)
                return;

            var existedTransaction = await _context.PaymentTransactions
                .AnyAsync(x => x.Provider == "vnpay"
                            && x.TransactionId == transactionNo);

            if (!existedTransaction)
            {
                _context.PaymentTransactions.Add(new PaymentTransaction
                {
                    Id = Guid.NewGuid(),
                    PaymentId = payment.Id,
                    Provider = "vnpay",
                    TransactionId = transactionNo,
                    RawResponse = JsonSerializer.Serialize(rawData),
                    CreatedAt = DateTime.UtcNow
                });
            }

            var vnpAmount = long.Parse(rawData["vnp_Amount"]) / 100;

            if (payment.Amount != vnpAmount)
            {
                payment.Status = PaymentStatus.failed;
                payment.Order.Status = OrderStatus.failed;
                payment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return;
            }

            if (responseCode == "00")
            {
                payment.Status = PaymentStatus.success;
                payment.PaidAt = DateTime.UtcNow;
                payment.Order.Status = OrderStatus.paid;

                await UpdateContentStatsAsync(payment.Order);
                await ProcessCommissionsAsync(payment.Order);
            }
            else
            {
                payment.Status = PaymentStatus.failed;
                payment.Order.Status = OrderStatus.failed;
            }

            payment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }


        public async Task<OrderDto?> GetByIdAsync(Guid id)
            => await MapToOrderDtoAsync(id);

        public async Task<List<OrderDto>> GetUserOrdersAsync(Guid userId)
        {
            var ids = await _context.Orders
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.Id)
                .ToListAsync();

            var result = new List<OrderDto>();

            foreach (var id in ids)
            {
                var dto = await MapToOrderDtoAsync(id);
                if (dto != null)
                    result.Add(dto);
            }

            return result;
        }

        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
                return false;

            order.Status = OrderStatus.cancelled;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task ProcessCommissionsAsync(Order order)
        {
            var items = await _context.OrderItems
                .Where(x => x.OrderId == order.Id && x.CollaboratorId != null)
                .Include(x => x.Collaborator!)
                .ToListAsync();

            foreach (var i in items)
            {
                var rate = i.Collaborator!.CommissionRate ?? 10;
                var amount = i.Price * i.Quantity * rate / 100;

                if (amount <= 0)
                    continue;

                var wallet = await _context.Wallets.FirstOrDefaultAsync(x =>
                    x.OwnerType == WalletOwnerType.collaborator &&
                    x.OwnerId == i.CollaboratorId);

                if (wallet == null)
                {
                    wallet = new Wallet
                    {
                        Id = Guid.NewGuid(),
                        OwnerType = WalletOwnerType.collaborator,
                        OwnerId = i.CollaboratorId!.Value,
                        Balance = 0
                    };
                    _context.Wallets.Add(wallet);
                }

                wallet.Balance += amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                _context.WalletTransactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,
                    Type = WalletTxType.commission,
                    Amount = amount,
                    ReferenceId = order.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task GrantDownloadsAsync(Order order)
        {
            var userId = order.UserId!.Value;

            // Chỉ grant downloads nếu chưa được grant cho order này
            var hasGrantedDownloads = await _context.Downloads
                .AnyAsync(x => x.UserId == userId && order.OrderItems.Select(oi => oi.ContentId).Contains(x.ContentId)
                    && x.CreatedAt >= order.CreatedAt);

            if (hasGrantedDownloads)
                return;

            foreach (var item in order.OrderItems)
            {
                _context.Downloads.Add(new Download
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ContentId = item.ContentId,
                    DownloadCount = 5 * item.Quantity,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task<OrderDto?> MapToOrderDtoAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Payment)
                    .ThenInclude(p => p!.PaymentTransactions)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Content)
                        .ThenInclude(c => c.Files)
                .Include(o => o.OrderCoupon)
                    .ThenInclude(oc => oc!.Coupon)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return null;

            var subtotal = order.OrderItems.Sum(x => x.Price * x.Quantity);

            var payment = order.Payment;
            var transactions = payment?.PaymentTransactions ?? new List<PaymentTransaction>();

            var paymentUrl = transactions
                .Where(x => x.PaymentUrl != null)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault()?.PaymentUrl;

            var transactionNo = transactions
                .Where(x => x.TransactionId != null)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault()?.TransactionId;

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                Subtotal = subtotal,
                DiscountAmount = order.OrderCoupon?.DiscountAmount ?? 0,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency!,
                Status = order.Status,
                CreatedAt = order.CreatedAt!.Value,
                PaidAt = payment?.PaidAt,
                Items = order.OrderItems.Select(x => new OrderItemDto
                {
                    ContentId = x.ContentId,
                    ContentTitle = x.Content!.Title,
                    Quantity = x.Quantity,
                    UnitPrice = x.Price,
                    Subtotal = x.Price * x.Quantity,
                    ImageUrl = x.Content.Files
                        .OrderBy(f => f.DisplayOrder)
                        .FirstOrDefault()?.FileUrl ?? ""
                }).ToList(),
                Payment = payment == null ? null : new PaymentDto
                {
                    Id = payment.Id,
                    Method = payment.PaymentMethod,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    PaymentUrl = paymentUrl,
                    TransactionNo = transactionNo
                }
            };
        }

        private async Task UpdateContentStatsAsync(Order order)
        {
            foreach (var id in order.OrderItems.Select(x => x.ContentId).Distinct())
            {
                var updated = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE content_stats SET purchases = purchases + 1 WHERE content_id = {0}",
                    id);

                if (updated == 0)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO content_stats (content_id, views, downloads, purchases) VALUES ({0},0,0,1)",
                        id);
                }
            }
        }
    }
}
