using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class CollaboratorRequest
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }
    public CollaboratorRequestStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User? User { get; set; }
}
