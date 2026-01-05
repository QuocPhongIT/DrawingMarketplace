using DrawingMarketplace.Domain.Enums;

namespace DrawingMarketplace.Application.DTOs.Collaborator
{
    public class UpdateCollaboratorStatusRequest
    {
        public CollaboratorRequestStatus Status { get; set; }
    }
}
