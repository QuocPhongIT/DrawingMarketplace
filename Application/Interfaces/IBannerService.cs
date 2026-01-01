using DrawingMarketplace.Application.DTOs.Banner;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IBannerService
    {
        Task<List<BannerDto>> GetActiveBannersAsync();
        Task<List<BannerDto>> GetAllBannersAsync();
        Task<BannerDto> CreateBannerAsync(CreateBannerDto dto, Guid currentUserId);
        Task<BannerDto> UpdateBannerAsync(Guid id, UpdateBannerDto dto, Guid currentUserId);
        Task DeleteBannerAsync(Guid id);
    }
}
