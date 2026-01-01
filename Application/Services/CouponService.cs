using DrawingMarketplace.Application.DTOs.Coupon;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using static DrawingMarketplace.Application.DTOs.Coupon.CouponUpsertDto;

namespace DrawingMarketplace.Application.Services
{
    public class CouponService : ICouponService
    {
        private readonly DrawingMarketplaceContext _context;

        public CouponService(DrawingMarketplaceContext context)
        {
            _context = context;
        }

        public async Task<CouponDto?> GetByCodeAsync(string code)
        {
            var coupon = await _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());

            return coupon == null ? null : MapToDto(coupon);
        }

        public async Task<CouponDto> CreateAsync(CreateCouponDto dto)
        {
            var existing = await _context.Coupons
                .AnyAsync(c => c.Code.ToLower() == dto.Code.ToLower());

            if (existing)
                throw new ConflictException("Mã coupon đã tồn tại");

            var coupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = dto.Code,
                Type = dto.Type,
                Value = dto.Value,
                MaxDiscount = dto.MaxDiscount,
                MinOrderAmount = dto.MinOrderAmount ?? 0,
                UsageLimit = dto.UsageLimit,
                UsedCount = 0,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return MapToDto(coupon);
        }

        public async Task<CouponDto?> UpdateAsync(Guid id, UpdateCouponDto dto)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return null;

            if (dto.Type.HasValue) coupon.Type = dto.Type.Value;
            if (dto.Value.HasValue) coupon.Value = dto.Value.Value;
            if (dto.MaxDiscount.HasValue) coupon.MaxDiscount = dto.MaxDiscount;
            if (dto.MinOrderAmount.HasValue) coupon.MinOrderAmount = dto.MinOrderAmount.Value;
            if (dto.UsageLimit.HasValue) coupon.UsageLimit = dto.UsageLimit;
            if (dto.ValidFrom.HasValue) coupon.ValidFrom = dto.ValidFrom;
            if (dto.ValidTo.HasValue) coupon.ValidTo = dto.ValidTo;
            if (dto.IsActive.HasValue) coupon.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();
            return MapToDto(coupon);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return false;

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CouponDto>> GetAllAsync()
        {
            return await _context.Coupons
                .AsNoTracking()
                .Select(c => MapToDto(c))
                .ToListAsync();
        }

        public async Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount)
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());

            if (coupon == null || !await ValidateCouponAsync(code, orderAmount))
                return 0;

            decimal discount = 0;

            if (coupon.Type == CouponType.percent)
            {
                discount = orderAmount * coupon.Value / 100;
                if (coupon.MaxDiscount.HasValue && discount > coupon.MaxDiscount.Value)
                    discount = coupon.MaxDiscount.Value;
            }
            else
            {
                discount = coupon.Value;
            }

            return Math.Min(discount, orderAmount);
        }

        public async Task<bool> ValidateCouponAsync(string code, decimal orderAmount)
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());

            if (coupon == null) return false;

            if (coupon.IsActive == false) return false;

            var now = DateTime.UtcNow;

            if (coupon.ValidFrom.HasValue && now.Date < coupon.ValidFrom.Value.Date)
                return false;

            if (coupon.ValidTo.HasValue && now.Date > coupon.ValidTo.Value.Date)
                return false;

            if (orderAmount < (coupon.MinOrderAmount ?? 0m))
                return false;

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return false;

            return true;
        }

        public async Task ApplyCouponAsync(Guid couponId, Guid orderId)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon == null)
                throw new NotFoundException("Coupon", couponId);

            coupon.UsedCount = (coupon.UsedCount ?? 0) + 1;

            var orderCoupon = new OrderCoupon
            {
                OrderId = orderId,
                CouponId = couponId,
                DiscountAmount = 0,
                AppliedAt = DateTime.UtcNow
            };

            _context.OrderCoupons.Add(orderCoupon);
            await _context.SaveChangesAsync();
        }

        private static CouponDto MapToDto(Coupon coupon) => new()
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Type = coupon.Type,
            Value = coupon.Value,
            MaxDiscount = coupon.MaxDiscount,
            MinOrderAmount = coupon.MinOrderAmount,
            UsageLimit = coupon.UsageLimit,
            UsedCount = coupon.UsedCount,
            ValidFrom = coupon.ValidFrom,
            ValidTo = coupon.ValidTo,
            IsActive = coupon.IsActive ?? false,
            CreatedAt = coupon.CreatedAt
        };
    }
}

