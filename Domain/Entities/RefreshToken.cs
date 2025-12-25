using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class RefreshToken
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiredAt { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByToken { get; set; }

    public string? IpAddress { get; set; }

    public string? Device { get; set; }

    public virtual User? User { get; set; }
    public bool IsRevoked => RevokedAt != null || ExpiredAt <= DateTime.UtcNow;
}
