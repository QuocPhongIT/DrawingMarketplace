using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.Order
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? PaymentUrl { get; set; }
    }
}


