using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.Order
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public string Method { get; set; } = string.Empty; 
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? PaymentUrl { get; set; }
        public string? TransactionNo { get; set; }
    }
}


