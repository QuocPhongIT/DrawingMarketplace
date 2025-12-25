using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class Download
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? ContentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Content? Content { get; set; }

    public virtual User? User { get; set; }
}
