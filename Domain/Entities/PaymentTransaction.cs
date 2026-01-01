using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class PaymentTransaction
{
    public Guid Id { get; set; }

    public Guid? PaymentId { get; set; }

    public string? Provider { get; set; }

    public string? TransactionId { get; set; }

    public string? RawResponse { get; set; }

    public DateTime? CreatedAt { get; set; }
    public string? PaymentUrl { get; set; }

    public virtual Payment? Payment { get; set; }
}
