namespace DrawingMarketplace.Domain.Interfaces
{
    public interface IPaymentGateway
    {
        Task<PaymentResult> CreatePaymentAsync(CreatePaymentRequest request);
        PaymentStatusResult ParsePaymentResult(IDictionary<string, string> queryParams);
        bool VerifySignature(IDictionary<string, string> queryParams);
    }

    public class CreatePaymentRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string Description { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? PaymentUrl { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PaymentStatusResult
    {
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public string? BankCode { get; set; }
        public string? ResponseCode { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RefundResult
    {
        public bool Success { get; set; }
        public string? RefundTransactionId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
