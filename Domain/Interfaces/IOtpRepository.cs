using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Domain.Interfaces
{
    public interface IOtpRepository
    {
        Task AddAsync(Otp otp);
        Task<Otp?> GetLatestOtpAsync(string email, OtpType type);
        Task UpdateAsync(Otp otp);
        Task InvalidateAsync(string email, OtpType type);
    }
}
