using System.Net;
using System.Net.Mail;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MimeKit;


namespace DrawingMarketplace.Infrastructure.Services
{
    public sealed class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendOtpAsync(string email, string otp)
        {
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.Username));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Your OTP Code";

            var bodyBuilder = new MimeKit.BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <h2 style='color: #0a84ff;'>Drawing Marketplace</h2>
                    <p>Use the following OTP to verify your account:</p>
                    <p style='font-size: 24px; font-weight: bold; letter-spacing: 2px; background-color: #f0f0f0; display: inline-block; padding: 10px 20px; border-radius: 5px;'>{otp}</p>
                    <p>This code will expire in 10 minutes.</p>
                    <p>If you did not request this, please ignore this email.</p>
                </div>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
