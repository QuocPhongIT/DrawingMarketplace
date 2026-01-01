using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid? OrderId { get; set; }

    public Guid? UserId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public PaymentStatus Status { get; set; } = PaymentStatus.pending;

    public DateTime? CreatedAt { get; set; }

    public virtual Order? Order { get; set; }

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual User? User { get; set; }
}
