using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class CopyrightReport
{
    public Guid Id { get; set; }

    public Guid? ContentId { get; set; }

    public Guid? ReporterId { get; set; }

    public string? Reason { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.pending;
    public DateTime? CreatedAt { get; set; }

    public virtual Content? Content { get; set; }

    public virtual User? Reporter { get; set; }
}
