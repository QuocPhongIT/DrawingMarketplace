using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrawingMarketplace.Domain.Entities;

public partial class Wallet
{
    public Guid Id { get; set; }

    [Column("owner_type")]
    public WalletOwnerType OwnerType { get; set; }

    public Guid OwnerId { get; set; }

    public decimal? Balance { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
