using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class CollaboratorBank
{
    public Guid Id { get; set; }

    public Guid? CollaboratorId { get; set; }

    public string BankName { get; set; } = null!;

    public string BankAccount { get; set; } = null!;

    public string OwnerName { get; set; } = null!;

    public bool? IsDefault { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Collaborator? Collaborator { get; set; }

    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
}
