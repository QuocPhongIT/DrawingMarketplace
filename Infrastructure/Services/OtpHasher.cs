using System.Security.Cryptography;
using System.Text;

namespace DrawingMarketplace.Infrastructure.Services
{
    public static class OtpHasher
    {
        public static string Hash(string otp, string secret)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes($"{otp}:{secret}");
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash); 
        }
    }
}
