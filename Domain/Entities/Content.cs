using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class Content: BaseEntity
{
    public Guid Id { get; set; }

    public Guid? CollaboratorId { get; set; }

    public Guid? CategoryId { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.draft;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category? Category { get; set; }

    public virtual Collaborator? Collaborator { get; set; }

    public virtual ContentStat? ContentStat { get; set; }

    public virtual ICollection<CopyrightReport> CopyrightReports { get; set; } = new List<CopyrightReport>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Download> Downloads { get; set; } = new List<Download>();

    public virtual ICollection<MediaFile> Files { get; set; } = new List<MediaFile>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
