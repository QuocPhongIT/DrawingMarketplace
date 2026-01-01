namespace DrawingMarketplace.Application.DTOs.Wallet
{
    public class WalletDto
    {
        public Guid Id { get; set; }
        public decimal Balance { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class WalletTransactionDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Guid? ReferenceId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class WalletStatsDto
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public decimal PendingWithdrawal { get; set; }
    }
}


