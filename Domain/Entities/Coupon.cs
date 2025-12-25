using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class Coupon
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public decimal Value { get; set; }

    public decimal? MaxDiscount { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public int? UsageLimit { get; set; }

    public int? UsedCount { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<OrderCoupon> OrderCoupons { get; set; } = new List<OrderCoupon>();
}
