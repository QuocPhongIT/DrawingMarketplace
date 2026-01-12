namespace DrawingMarketplace.Domain.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpAsync(string email, string otp);
        Task SendWithdrawalCreatedAsync(string email, decimal amount);
        Task SendWithdrawalApprovedAsync(string email, decimal amount, decimal finalAmount);
        Task SendWithdrawalRejectedAsync(string email, decimal amount, string? reason = null);
        Task SendWithdrawalPaidAsync(string email, decimal amount);
        Task SendCopyrightReportProcessedAsync(
            string email,
            string contentTitle,
            string outcome,  
            string message
        );
    }
}
