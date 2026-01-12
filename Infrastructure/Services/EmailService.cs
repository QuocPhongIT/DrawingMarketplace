using DrawingMarketplace.Domain.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace DrawingMarketplace.Infrastructure.Services
{
    public sealed class EmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly SendGridClient _client;

        public EmailService(IConfiguration configuration)
        {
            _apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
                ?? throw new InvalidOperationException("SENDGRID_API_KEY is not configured.");

            _client = new SendGridClient(_apiKey);
        }

        private EmailAddress GetFromEmail() => new EmailAddress("phongindra@gmail.com", "Drawing Marketplace");

        private async Task SendEmailAsync(string toEmail, string subject, string plainText, string htmlContent)
        {
            var from = GetFromEmail();
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);

            var response = await _client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid error: {response.StatusCode} - {errorBody}");
            }
        }

        public async Task SendOtpAsync(string email, string otp)
        {
            var subject = "Mã xác thực tài khoản Drawing Marketplace";

            var plainTextContent = $@"Xin chào,
                Mã xác thực (OTP) của bạn là: {otp}
                Mã này có hiệu lực trong vòng 10 phút.
                Vui lòng không chia sẻ mã này cho bất kỳ ai.
                Nếu bạn không yêu cầu tạo tài khoản, hãy bỏ qua email này.
                Trân trọng,
                Drawing Marketplace";

                            var htmlContent = $@"
                <div style='font-family: Arial, Helvetica, sans-serif; background-color:#f4f6f8; padding:40px'>
                  <div style='max-width:600px; margin:auto; background:#ffffff; padding:32px; border-radius:8px'>
                    <h2 style='color:#111827; margin-bottom:16px'>Xác thực tài khoản Drawing Marketplace</h2>
                    <p style='font-size:15px; color:#374151'>
                      Xin chào,<br/><br/>
                      Bạn đang thực hiện đăng ký hoặc xác minh tài khoản tại <b>Drawing Marketplace</b>.
                    </p>
                    <p style='font-size:15px; color:#374151'>Mã xác thực (OTP) của bạn là:</p>
                    <div style='margin:24px 0; text-align:center'>
                      <span style='display:inline-block; font-size:32px; letter-spacing:8px; font-weight:600;
                                   background:#f9fafb; padding:16px 32px; border-radius:6px; color:#111827'>
                        {otp}
                      </span>
                    </div>
                    <p style='font-size:14px; color:#6b7280'>
                      Mã có hiệu lực trong <b>10 phút</b>. Vui lòng không chia sẻ mã này cho bất kỳ ai.
                    </p>
                    <hr style='border:none; border-top:1px solid #e5e7eb; margin:24px 0'/>
                    <p style='font-size:13px; color:#9ca3af'>
                      Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.<br/>
                      Email này được gửi tự động, vui lòng không trả lời.
                    </p>
                    <p style='font-size:13px; color:#9ca3af'>© {DateTime.UtcNow.Year} Drawing Marketplace</p>
                  </div>
                </div>";

            await SendEmailAsync(email, subject, plainTextContent, htmlContent);
        }

        public async Task SendWithdrawalCreatedAsync(string email, decimal amount)
        {
            var subject = "Yêu cầu rút tiền đã được tạo thành công";

            var plainText = $@"Xin chào,
                Yêu cầu rút {amount:N0} VNĐ của bạn đã được tạo thành công.
                Chúng tôi sẽ xem xét và xử lý trong thời gian sớm nhất.
                Trân trọng,
                Drawing Marketplace";

                            var html = $@"
                <div style='font-family: Arial, Helvetica, sans-serif; background-color:#f4f6f8; padding:40px'>
                  <div style='max-width:600px; margin:auto; background:#ffffff; padding:32px; border-radius:8px'>
                    <h2 style='color:#111827; margin-bottom:16px'>Yêu cầu rút tiền đã được tạo</h2>
                    <p style='font-size:15px; color:#374151'>
                      Xin chào,<br/><br/>
                      Yêu cầu rút tiền <b>{amount:N0} VNĐ</b> của bạn đã được gửi thành công.
                    </p>
                    <p style='font-size:15px; color:#374151'>
                      Chúng tôi sẽ xem xét và thông báo kết quả sớm nhất có thể.
                    </p>
                    <hr style='border:none; border-top:1px solid #e5e7eb; margin:24px 0'/>
                    <p style='font-size:13px; color:#9ca3af'>
                      Nếu bạn không thực hiện yêu cầu này, vui lòng liên hệ hỗ trợ ngay.<br/>
                      Email này được gửi tự động.
                    </p>
                    <p style='font-size:13px; color:#9ca3af'>© {DateTime.UtcNow.Year} Drawing Marketplace</p>
                  </div>
                </div>";

            await SendEmailAsync(email, subject, plainText, html);
        }

        public async Task SendWithdrawalApprovedAsync(string email, decimal amount, decimal finalAmount)
        {
            var subject = "Yêu cầu rút tiền của bạn đã được duyệt";

            var plainText = $@"Xin chào,
                Yêu cầu rút {amount:N0} VNĐ đã được duyệt.
                Số tiền thực nhận ước tính: {finalAmount:N0} VNĐ (sau phí nếu có).
                Tiền sẽ được chuyển khoản trong thời gian sớm nhất.
                Trân trọng,
                Drawing Marketplace";

                            var html = $@"
                <div style='font-family: Arial, Helvetica, sans-serif; background-color:#f4f6f8; padding:40px'>
                  <div style='max-width:600px; margin:auto; background:#ffffff; padding:32px; border-radius:8px'>
                    <h2 style='color:#111827; margin-bottom:16px'>Yêu cầu rút tiền đã được duyệt</h2>
                    <p style='font-size:15px; color:#374151'>
                      Chúc mừng bạn!<br/><br/>
                      Yêu cầu rút <b>{amount:N0} VNĐ</b> đã được duyệt thành công.
                    </p>
                    <p style='font-size:15px; color:#374151'>
                      Số tiền thực nhận ước tính: <b>{finalAmount:N0} VNĐ</b> (sau khi trừ phí chuyển khoản nếu áp dụng).
                    </p>
                    <p style='font-size:15px; color:#374151'>
                      Tiền sẽ được chuyển về tài khoản ngân hàng bạn đã đăng ký trong thời gian sớm nhất.
                    </p>
                    <hr style='border:none; border-top:1px solid #e5e7eb; margin:24px 0'/>
                    <p style='font-size:13px; color:#9ca3af'>© {DateTime.UtcNow.Year} Drawing Marketplace</p>
                  </div>
                </div>";

            await SendEmailAsync(email, subject, plainText, html);
        }

        public async Task SendWithdrawalRejectedAsync(string email, decimal amount, string? reason = null)
        {
            var subject = "Yêu cầu rút tiền của bạn bị từ chối";

            var reasonText = string.IsNullOrEmpty(reason) ? "" : $"\nLý do: {reason}";
            var plainText = $@"Xin chào,
                Yêu cầu rút {amount:N0} VNĐ của bạn đã bị từ chối.{reasonText}
                Vui lòng kiểm tra thông tin và tạo yêu cầu mới nếu cần.
                Trân trọng,
                Drawing Marketplace";

                            var html = $@"
                <div style='font-family: Arial, Helvetica, sans-serif; background-color:#f4f6f8; padding:40px'>
                  <div style='max-width:600px; margin:auto; background:#ffffff; padding:32px; border-radius:8px'>
                    <h2 style='color:#111827; margin-bottom:16px'>Yêu cầu rút tiền bị từ chối</h2>
                    <p style='font-size:15px; color:#374151'>
                      Xin chào,<br/><br/>
                      Yêu cầu rút <b>{amount:N0} VNĐ</b> của bạn đã bị từ chối.
                    </p>
                    {(string.IsNullOrEmpty(reason) ? "" : $"<p style='font-size:15px; color:#374151'>Lý do: <b>{reason}</b></p>")}
                    <p style='font-size:15px; color:#374151'>
                      Vui lòng kiểm tra lại thông tin ngân hàng hoặc số dư ví và tạo yêu cầu mới.
                    </p>
                    <hr style='border:none; border-top:1px solid #e5e7eb; margin:24px 0'/>
                    <p style='font-size:13px; color:#9ca3af'>© {DateTime.UtcNow.Year} Drawing Marketplace</p>
                  </div>
                </div>";

            await SendEmailAsync(email, subject, plainText, html);
        }

        public async Task SendWithdrawalPaidAsync(string email, decimal amount)
        {
            var subject = "Tiền rút đã được chuyển khoản thành công";

            var plainText = $@"Xin chào,
                Yêu cầu rút {amount:N0} VNĐ của bạn đã được thanh toán thành công.
                Tiền đã được chuyển về tài khoản ngân hàng bạn đăng ký.
                Cảm ơn bạn đã đồng hành cùng Drawing Marketplace!
                Trân trọng,
                Drawing Marketplace";

                            var html = $@"
                <div style='font-family: Arial, Helvetica, sans-serif; background-color:#f4f6f8; padding:40px'>
                  <div style='max-width:600px; margin:auto; background:#ffffff; padding:32px; border-radius:8px'>
                    <h2 style='color:#111827; margin-bottom:16px'>Thanh toán rút tiền thành công</h2>
                    <p style='font-size:15px; color:#374151'>
                      Chúc mừng bạn!<br/><br/>
                      Số tiền <b>{amount:N0} VNĐ</b> đã được chuyển khoản thành công về tài khoản ngân hàng.
                    </p>
                    <p style='font-size:15px; color:#374151'>
                      Bạn có thể kiểm tra tài khoản để xác nhận.
                    </p>
                    <p style='font-size:15px; color:#374151'>
                      Cảm ơn bạn đã đồng hành cùng <b>Drawing Marketplace</b>!
                    </p>
                    <hr style='border:none; border-top:1px solid #e5e7eb; margin:24px 0'/>
                    <p style='font-size:13px; color:#9ca3af'>© {DateTime.UtcNow.Year} Drawing Marketplace</p>
                  </div>
                </div>";

            await SendEmailAsync(email, subject, plainText, html);
        }

        public async Task SendCopyrightReportProcessedAsync(string email, string contentTitle, string outcome, string message)
        {
            var subject = $"Cập nhật báo cáo bản quyền: {contentTitle}";

            var plainText = $@"Xin chào,
                {message}
                Trạng thái: {outcome.ToUpper()}
                Trân trọng,
                Drawing Marketplace";

                            var html = $@"
                <div style='font-family: Arial, Helvetica, sans-serif; background-color:#f4f6f8; padding:40px'>
                  <div style='max-width:600px; margin:auto; background:#ffffff; padding:32px; border-radius:8px'>
                    <h2 style='color:#111827; margin-bottom:16px'>Cập nhật báo cáo bản quyền</h2>
                    <p style='font-size:15px; color:#374151'>
                      Xin chào,<br/><br/>
                      {message}
                    </p>
                    <p style='font-size:15px; color:#374151'>
                      Nội dung: <b>{contentTitle}</b><br/>
                      Trạng thái: <b>{outcome.ToUpper()}</b>
                    </p>
                    <hr style='border:none; border-top:1px solid #e5e7eb; margin:24px 0'/>
                    <p style='font-size:13px; color:#9ca3af'>
                      Nếu bạn có thắc mắc, vui lòng liên hệ hỗ trợ.<br/>
                      Email này được gửi tự động.
                    </p>
                    <p style='font-size:13px; color:#9ca3af'>© {DateTime.UtcNow.Year} Drawing Marketplace</p>
                  </div>
                </div>";

            await SendEmailAsync(email, subject, plainText, html);
        }
    }
}