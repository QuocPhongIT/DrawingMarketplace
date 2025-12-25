using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class Wallet
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public decimal? Balance { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
