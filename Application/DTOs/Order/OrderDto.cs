using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.Order
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Currency { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public PaymentDto? Payment { get; set; }
        public CouponDto? Coupon { get; set; }
    }

    public class OrderItemDto
    {
        public Guid ContentId { get; set; }
        public string ContentTitle { get; set; } = string.Empty;
        public Guid? CollaboratorId { get; set; }
        public decimal Price { get; set; }
    }
}


