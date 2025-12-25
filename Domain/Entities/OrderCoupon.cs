using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class OrderCoupon
{
    public Guid OrderId { get; set; }

    public Guid? CouponId { get; set; }

    public decimal DiscountAmount { get; set; }

    public DateTime? AppliedAt { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual Order Order { get; set; } = null!;
}
