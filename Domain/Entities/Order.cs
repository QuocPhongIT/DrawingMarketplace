using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class Order
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Currency { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual OrderCoupon? OrderCoupon { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Payment? Payment { get; set; }

    public virtual User? User { get; set; }
}
