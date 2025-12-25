using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class CartItem
{
    public Guid CartId { get; set; }

    public Guid ContentId { get; set; }

    public decimal Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Content Content { get; set; } = null!;
}
