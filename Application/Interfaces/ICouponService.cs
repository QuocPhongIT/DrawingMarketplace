using DrawingMarketplace.Application.DTOs.Coupon;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface ICouponService
    {
        Task<CouponDto?> GetByCodeAsync(string code);
        Task<CouponDto> CreateAsync(CouponUpsertDto.CreateCouponDto dto);
        Task<CouponDto?> UpdateAsync(Guid id, CouponUpsertDto.UpdateCouponDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<List<CouponDto>> GetAllAsync();
        Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount);
        Task<bool> ValidateCouponAsync(string code, decimal orderAmount);
        Task ApplyCouponAsync(Guid couponId, Guid orderId);
    }
}

