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
            var vnpParams = new SortedList<string, string>
    {
        { "vnp_Version", "2.1.0" },
        { "vnp_Command", "pay" },
        { "vnp_TmnCode", _settings.TmnCode },
        { "vnp_Amount", ((long)(request.Amount * 100)).ToString() },
        { "vnp_CurrCode", "VND" },
        { "vnp_TxnRef", request.OrderId },
        { "vnp_OrderInfo", request.Description },
        { "vnp_OrderType", "other" },
        { "vnp_Locale", "vn" },
        { "vnp_ReturnUrl", _settings.ReturnUrl },
        { "vnp_IpAddr", "127.0.0.1" },
        { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
    };

            var hashData = string.Join("&",
                vnpParams.Select(x =>
                    $"{x.Key}={WebUtility.UrlEncode(x.Value)}")
            );

            var secureHash = HmacSHA512(_settings.HashSecret, hashData);
            var paymentUrl =
                $"{_settings.PaymentUrl}?" +
                $"{hashData}" +
                $"&vnp_SecureHashType=HmacSHA512" +
                $"&vnp_SecureHash={secureHash}";

            return Task.FromResult(new PaymentResult
            {
                Success = true,
                PaymentUrl = paymentUrl,
                TransactionId = request.OrderId
            });
        }


        public PaymentStatusResult ParsePaymentResult(IDictionary<string, string> queryParams)
        {
            if (!VerifySignature(queryParams))
            {
                return new PaymentStatusResult
                {
                    Success = false,
                    Status = "invalid_signature",
                    ErrorMessage = "Sai chữ ký"
                };
            }

            queryParams.TryGetValue("vnp_ResponseCode", out var code);
            queryParams.TryGetValue("vnp_TxnRef", out var txnRef);
            queryParams.TryGetValue("vnp_Amount", out var amount);

            var success = code == "00";

            return new PaymentStatusResult
            {
                Success = success,
                Status = success ? "success" : "failed",
                TransactionId = txnRef,
                Amount = amount != null ? decimal.Parse(amount) / 100 : null,
                ResponseCode = code
            };
        }

        public bool VerifySignature(IDictionary<string, string> queryParams)
        {
            if (!queryParams.TryGetValue("vnp_SecureHash", out var secureHash))
                return false;

            var hashData = string.Join("&",
                queryParams
                    .Where(x =>
                        x.Key.StartsWith("vnp_") &&
                        x.Key != "vnp_SecureHash" &&
                        x.Key != "vnp_SecureHashType")
                    .OrderBy(x => x.Key)
                    .Select(x =>
                        $"{x.Key}={WebUtility.UrlEncode(x.Value)}")
            );

            var checkHash = HmacSHA512(_settings.HashSecret, hashData);
            return checkHash.Equals(secureHash, StringComparison.OrdinalIgnoreCase);
        }

        private string HmacSHA512(string key, string input)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
    }
}
