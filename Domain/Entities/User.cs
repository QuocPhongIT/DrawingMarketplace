using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;

namespace DrawingMarketplace.Domain.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
    public UserStatus Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual Collaborator? Collaborator { get; set; }

    public virtual ICollection<CollaboratorRequest> CollaboratorRequestApprovedByNavigations { get; set; } = new List<CollaboratorRequest>();

    public virtual ICollection<CollaboratorRequest> CollaboratorRequestUsers { get; set; } = new List<CollaboratorRequest>();

    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();

    public virtual ICollection<CopyrightReport> CopyrightReports { get; set; } = new List<CopyrightReport>();

    public virtual ICollection<Download> Downloads { get; set; } = new List<Download>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Otp> Otps { get; set; } = new List<Otp>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
}
