using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.Collaborator
{
    public class CollaboratorDto
    {
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public CollaboratorActivityStatus Status { get; set; }

        public decimal? CommissionRate { get; set; }

        public DateTime? CreatedAt { get; set; }
        public bool HasBankAccount => Banks != null && Banks.Any();

        public IReadOnlyList<CollaboratorBankDto> Banks { get; set; }
            = new List<CollaboratorBankDto>();
    }
}
