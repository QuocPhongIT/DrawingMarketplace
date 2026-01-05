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

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                    throw new Exception("Giỏ hàng không tồn tại");

                if (!cart.CartItems.Any())
                    throw new Exception("Giỏ hàng trống");

                var contentMap = await _context.Contents
                    .Where(c => cart.CartItems.Select(ci => ci.ContentId).Contains(c.Id))
                    .Select(c => new { c.Id, c.CollaboratorId })
                    .ToDictionaryAsync(x => x.Id, x => x.CollaboratorId);

                foreach (var cartItem in cart.CartItems)
                {
                    if (!contentMap.TryGetValue(cartItem.ContentId, out var collaboratorId))
                        continue;

                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        ContentId = cartItem.ContentId,
                        Price = cartItem.Price,
                        CollaboratorId = collaboratorId
                    });
                }

                _context.CartItems.RemoveRange(cart.CartItems);
                _context.Carts.Remove(cart);

                await _context.SaveChangesAsync();

                if (dto.PaymentMethod.Equals("vnpay", StringComparison.OrdinalIgnoreCase))
                {
                    var paymentResult = await _paymentGatewayService
                        .GetGateway("vnpay")
                        .CreatePaymentAsync(new CreatePaymentRequest
                        {
                            OrderId = order.Id.ToString(),
                            Amount = payment.Amount,
                            Description = $"Thanh toán đơn hàng #{order.Id}"
                        });

                    if (!paymentResult.Success || string.IsNullOrEmpty(paymentResult.PaymentUrl))
                        throw new Exception("Không thể tạo link thanh toán VNPAY");

                    _context.PaymentTransactions.Add(new PaymentTransaction
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = payment.Id,
                        Provider = "vnpay",
                        TransactionId = paymentResult.TransactionId,
                        PaymentUrl = paymentResult.PaymentUrl,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await MapToOrderDtoAsync(order.Id)
                       ?? throw new Exception("Không thể lấy thông tin đơn hàng");
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

            var result = new List<OrderDto>();
            foreach (var order in orders)
            {
                var dto = await MapToOrderDtoAsync(order.Id);
                if (dto != null)
                    result.Add(dto);
            }
            return result;
        }

        public async Task ProcessPaymentCallbackAsync(Guid paymentId, string status, string transactionId)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.OrderItems)
                .Include(p => p.Order)
                    .ThenInclude(o => o.User)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null || payment.Order == null)
                throw new NotFoundException("Payment", paymentId);

            if (status.ToLower() == "success")
            {
                if (payment.Status == PaymentStatus.success)
                    return;

                payment.Status = PaymentStatus.success;
                payment.Order.Status = OrderStatus.paid;

                await GrantDownloadsAsync(payment.Order);
                await ProcessCommissionsAsync(payment.Order);
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

        private async Task ProcessCommissionsAsync(Order order)
        {
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == order.Id && oi.CollaboratorId != null)
                .Include(oi => oi.Collaborator)
                .ToListAsync();

            foreach (var item in orderItems)
            {
                var collaborator = item.Collaborator;
                if (collaborator == null) continue;

                var rate = collaborator.CommissionRate.GetValueOrDefault(10m);
                if (rate <= 0 || item.Price <= 0) continue;

                var amount = item.Price * rate / 100m;
                if (amount <= 0) continue;

                var wallet = await _context.Wallets.FirstOrDefaultAsync(w =>
                    w.OwnerType == WalletOwnerType.collaborator &&
                    w.OwnerId == collaborator.Id);

                if (wallet == null)
                {
                    wallet = new Wallet
                    {
                        Id = Guid.NewGuid(),
                        OwnerType = WalletOwnerType.collaborator,
                        OwnerId = collaborator.Id,
                        Balance = 0,
                        UpdatedAt = DateTime.UtcNow
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
            var userId = order.UserId;

            var contentIds = order.OrderItems
                .Select(oi => oi.ContentId)
                .Distinct()
                .ToList();

            var existingContentIds = await _context.Downloads
                .Where(d => d.UserId == userId && contentIds.Contains(d.ContentId!.Value))
                .Select(d => d.ContentId!.Value)
                .ToListAsync();

            var downloads = contentIds
                .Except(existingContentIds)
                .Select(contentId => new Download
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ContentId = contentId,
                    CreatedAt = DateTime.UtcNow
                });

            await _context.Downloads.AddRangeAsync(downloads);
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
                Payment = order.Payment == null ? null : new PaymentDto
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
                },
                Coupon = order.OrderCoupon == null ? null : new CouponDto
                {
                    Code = order.OrderCoupon.Coupon?.Code ?? "",
                    DiscountAmount = order.OrderCoupon.DiscountAmount
                }
            };
        }
    }
}
