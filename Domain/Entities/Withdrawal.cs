using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class Withdrawal
{
    public Guid Id { get; set; }

    public Guid? CollaboratorId { get; set; }

    public Guid? BankId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public Guid? ProcessedBy { get; set; }

    public virtual CollaboratorBank? Bank { get; set; }

    public virtual Collaborator? Collaborator { get; set; }

    public virtual User? ProcessedByNavigation { get; set; }
}
