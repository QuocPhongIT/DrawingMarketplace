using DrawingMarketplace.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;
namespace DrawingMarketplace.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        public bool Verify(string password, string hash)
        {
            if (password == null || hash == null) return false;
            return Hash(password) == hash;
        }
    }
}
