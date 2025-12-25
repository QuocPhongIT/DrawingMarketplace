using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class MediaFile
{
    public Guid Id { get; set; }

    public Guid? ContentId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public string? FileType { get; set; }

    public long? Size { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Content? Content { get; set; }
}
