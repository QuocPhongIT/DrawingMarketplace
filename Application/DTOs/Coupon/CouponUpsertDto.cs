using DrawingMarketplace.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace DrawingMarketplace.Application.DTOs.Coupon
{
    public class CouponUpsertDto
    {
        public class CreateCouponDto
        {
            [Required]
            public string Code { get; set; } = string.Empty;
            [Required]
            public CouponType Type { get; set; }
            [Range(0.01, double.MaxValue)]
            public decimal Value { get; set; }
            public decimal? MaxDiscount { get; set; }
            public decimal? MinOrderAmount { get; set; }
            public int? UsageLimit { get; set; }
            public DateTime? ValidFrom { get; set; }
            public DateTime? ValidTo { get; set; }
            public bool IsActive { get; set; } = true;
        }

        public class UpdateCouponDto
        {
            public CouponType? Type { get; set; }
            public decimal? Value { get; set; }
            public decimal? MaxDiscount { get; set; }
            public decimal? MinOrderAmount { get; set; }
            public int? UsageLimit { get; set; }
            public DateTime? ValidFrom { get; set; }
            public DateTime? ValidTo { get; set; }
            public bool? IsActive { get; set; }
        }
    }
}


