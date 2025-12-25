namespace DrawingMarketplace.Domain.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpAsync(string email, string otp);
    }
}
