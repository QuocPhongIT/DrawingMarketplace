using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrawingMarketplace.Domain.Entities;

public partial class WalletTransaction
{
    public Guid Id { get; set; }

    public Guid? WalletId { get; set; }
    [Column("type")]
    public WalletTxType Type { get; set; }

    public decimal Amount { get; set; }

    public Guid? ReferenceId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Wallet? Wallet { get; set; }
}
