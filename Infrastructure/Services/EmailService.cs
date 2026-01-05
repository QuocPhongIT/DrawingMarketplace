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

            var from = new EmailAddress(
                "phongindra@gmail.com",
                "Drawing Marketplace"
            );

            var to = new EmailAddress(email);

            var subject = "Mã xác thực tài khoản Drawing Marketplace";

            var plainTextContent =
                $@"Xin chào,

                Mã xác thực (OTP) của bạn là: {otp}

                Mã này có hiệu lực trong vòng 10 phút.
                Vui lòng không chia sẻ mã này cho bất kỳ ai.

                Nếu bạn không yêu cầu tạo tài khoản, hãy bỏ qua email này.

                Trân trọng,
                Drawing Marketplace";

                            var htmlContent = $@"
                <div style='font-family: Arial, Helvetica, sans-serif; background-color:#f4f6f8; padding:40px'>
                  <div style='max-width:600px; margin:auto; background:#ffffff; padding:32px; border-radius:8px'>
    
                    <h2 style='color:#111827; margin-bottom:16px'>
                      Xác thực tài khoản Drawing Marketplace
                    </h2>

                    <p style='font-size:15px; color:#374151'>
                      Xin chào,<br/><br/>
                      Bạn đang thực hiện đăng ký hoặc xác minh tài khoản tại <b>Drawing Marketplace</b>.
                    </p>

                    <p style='font-size:15px; color:#374151'>
                      Mã xác thực (OTP) của bạn là:
                    </p>

                    <div style='margin:24px 0; text-align:center'>
                      <span style='display:inline-block; font-size:32px; letter-spacing:8px; font-weight:600;
                                   background:#f9fafb; padding:16px 32px; border-radius:6px; color:#111827'>
                        {otp}
                      </span>
                    </div>

                    <p style='font-size:14px; color:#6b7280'>
                      Mã có hiệu lực trong <b>10 phút</b>.  
                      Vui lòng không chia sẻ mã này cho bất kỳ ai.
                    </p>

                    <hr style='border:none; border-top:1px solid #e5e7eb; margin:24px 0'/>

                    <p style='font-size:13px; color:#9ca3af'>
                      Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.<br/>
                      Email này được gửi tự động, vui lòng không trả lời.
                    </p>

                    <p style='font-size:13px; color:#9ca3af'>
                      © {DateTime.UtcNow.Year} Drawing Marketplace
                    </p>

                  </div>
                </div>";

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent,
                htmlContent
            );

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid error: {response.StatusCode} - {errorBody}");
            }
        }
    }
}
