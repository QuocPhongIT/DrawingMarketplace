namespace DrawingMarketplace.Application.DTOs.Collaborator
{
    public class CollaboratorBankDto
    {
        public Guid Id { get; set; }

        public string BankName { get; set; } = null!;

        public string BankAccount { get; set; } = null!;

        public string OwnerName { get; set; } = null!;

        public bool IsDefault { get; set; }
    }
}
