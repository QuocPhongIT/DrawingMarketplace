using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace DrawingMarketplace.Infrastructure.Services.PaymentGateways
{
    public class VnPayGateway : IPaymentGateway
    {
        private readonly VnPaySettings _settings;

        public VnPayGateway(IOptions<VnPaySettings> settings)
        {
            _settings = settings.Value;
        }

        public Task<PaymentResult> CreatePaymentAsync(CreatePaymentRequest request)
        {
            var vnpParams = new SortedList<string, string>();
            vnpParams.Add("vnp_Version", "2.1.0");
            vnpParams.Add("vnp_Command", "pay");
            vnpParams.Add("vnp_TmnCode", _settings.TmnCode);
            vnpParams.Add("vnp_Amount", ((long)(request.Amount * 100)).ToString());
            vnpParams.Add("vnp_CurrCode", "VND");
            vnpParams.Add("vnp_TxnRef", request.OrderId);
            vnpParams.Add("vnp_OrderInfo", request.Description);
            vnpParams.Add("vnp_OrderType", "other");
            vnpParams.Add("vnp_Locale", "vn");
            vnpParams.Add("vnp_ReturnUrl", _settings.ReturnUrl + request.ReturnUrl);
            vnpParams.Add("vnp_IpAddr", "127.0.0.1");
            vnpParams.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));

            var paymentUrl = CreateRequestUrl(vnpParams, _settings.PaymentUrl, _settings.HashSecret);

            return Task.FromResult(new PaymentResult
            {
                Success = true,
                PaymentUrl = paymentUrl,
                TransactionId = request.OrderId
            });
        }

        public Task<PaymentStatusResult> CheckPaymentStatusAsync(string transactionId)
        {
            return Task.FromResult(new PaymentStatusResult
            {
                Success = true,
                Status = "pending",
                TransactionId = transactionId
            });
        }

        public Task<RefundResult> RefundAsync(string transactionId, decimal amount, string reason)
        {
            return Task.FromResult(new RefundResult
            {
                Success = true,
                RefundTransactionId = Guid.NewGuid().ToString()
            });
        }

        private string CreateRequestUrl(SortedList<string, string> requestData, string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();
            foreach (var kv in requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string queryString = data.ToString().TrimEnd('&');

            string vnpSecureHash = HmacSHA512(vnpHashSecret, queryString);

            return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnpSecureHash;
        }
        private string HmacSHA512(string key, string inputData)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);
            return string.Concat(hashBytes.Select(b => b.ToString("x2")));
        }
    }
}
