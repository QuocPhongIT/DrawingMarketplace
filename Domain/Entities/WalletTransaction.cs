using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class WalletTransaction
{
    public Guid Id { get; set; }

    public Guid? WalletId { get; set; }

    public decimal Amount { get; set; }

    public Guid? ReferenceId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Wallet? Wallet { get; set; }
}
