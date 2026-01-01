namespace DrawingMarketplace.Application.DTOs.Withdrawal
{
    public class CreateWithdrawalDto
    {
        public Guid BankId { get; set; }
        public decimal Amount { get; set; }
    }
}


