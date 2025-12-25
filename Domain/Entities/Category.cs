using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class Category : BaseEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid? ParentId { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();

    public virtual ICollection<Category> InverseParent { get; set; } = new List<Category>();

    public virtual Category? Parent { get; set; }
}
