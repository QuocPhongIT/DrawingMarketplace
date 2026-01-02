using DrawingMarketplace.Domain.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DrawingMarketplace.Infrastructure.Services
{
    public sealed class EmailService : IEmailService
    {
        private readonly string _apiKey;

        public EmailService(IConfiguration configuration)
        {
            _apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
                      ?? throw new InvalidOperationException("SENDGRID_API_KEY is not configured.");
        }

        public async Task SendOtpAsync(string email, string otp)
        {
            var client = new SendGridClient(_apiKey);

            var from = new EmailAddress("phongindra@gmail.com", "Drawing Marketplace");
            var to = new EmailAddress(email);
            var subject = "Your OTP Code - Drawing Marketplace";

            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto; padding: 30px; background-color: #f9f9f9; border-radius: 10px; text-align: center;'>
                    <h2 style='color: #0a84ff;'>Drawing Marketplace</h2>
                    <p style='font-size: 16px;'>Use the following OTP to verify your account:</p>
                    <div style='margin: 40px 0;'>
                        <span style='font-size: 36px; font-weight: bold; letter-spacing: 12px; background-color: #ffffff; padding: 20px 40px; border-radius: 12px; display: inline-block; box-shadow: 0 4px 10px rgba(0,0,0,0.1);'>
                            {otp}
                        </span>
                    </div>
                    <p style='font-size: 14px; color: #666;'>This code will expire in 10 minutes.</p>
                    <p style='font-size: 14px; color: #666;'>If you did not request this, please ignore this email.</p>
                </div>";

            var plainTextContent = $"Your OTP code: {otp}. This code will expire in 10 minutes.";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                Console.WriteLine($"SendGrid Error: {response.StatusCode} - {errorBody}");
                throw new Exception($"Failed to send OTP email: {response.StatusCode}");
            }
        }
    }
}