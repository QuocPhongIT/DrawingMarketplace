using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.Withdrawal
{
    public class WithdrawalDto
    {
        public Guid Id { get; set; }
        public Guid CollaboratorId { get; set; }
        public Guid BankId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public WithdrawalStatus Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}


