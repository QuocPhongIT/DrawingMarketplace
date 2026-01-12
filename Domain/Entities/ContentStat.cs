using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class ContentStat
{
    public Guid ContentId { get; set; }

    public int? Views { get; set; } = 0;

    public int? Downloads { get; set; } = 0;

    public int? Purchases { get; set; } = 0;

    public virtual Content Content { get; set; } = null!;
}
