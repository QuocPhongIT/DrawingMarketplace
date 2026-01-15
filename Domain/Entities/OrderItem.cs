using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class OrderItem
{
    public Guid OrderId { get; set; }

    public Guid ContentId { get; set; }

    public Guid? CollaboratorId { get; set; }

    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;

    public virtual Collaborator? Collaborator { get; set; }

    public virtual Content Content { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
