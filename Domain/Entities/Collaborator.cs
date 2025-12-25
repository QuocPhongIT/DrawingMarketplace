using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class Collaborator
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }
    public CollaboratorActivityStatus Status { get; set; }
    public decimal? CommissionRate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<CollaboratorBank> CollaboratorBanks { get; set; } = new List<CollaboratorBank>();

    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User? User { get; set; }

    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
}
