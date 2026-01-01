using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public class MediaFile : BaseEntity
{
    public Guid Id { get; set; }

    public Guid ContentId { get; set; }

    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!; 
    public string? FileType { get; set; }
    public long? Size { get; set; }


    public FilePurpose Purpose { get; set; } = FilePurpose.downloadable;
    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Content Content { get; set; } = null!;
}