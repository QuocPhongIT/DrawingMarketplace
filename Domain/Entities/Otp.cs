using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class Otp
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public OtpType Type { get; set; }
    public DateTime ExpiredAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Email { get; set; }
    public string CodeHash { get; set; } = null!;

    public bool IsUsed { get; set; } = false;

    public int? AttemptCount { get; set; }

    public virtual User? User { get; set; }
}
