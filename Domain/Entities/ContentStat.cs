using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class ContentStat: BaseEntity
{
    public Guid ContentId { get; set; }

    public int? Views { get; set; }

    public int? Downloads { get; set; }

    public int? Purchases { get; set; }

    public virtual Content Content { get; set; } = null!;
}
