using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class Role
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
