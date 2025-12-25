using DrawingMarketplace.Application.Features.Collaborators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DrawingMarketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollaboratorController : ControllerBase
{
    private readonly ApplyCollaboratorHandler _applyCollaborator;
    private readonly ApproveCollaboratorHandler _approveCollaborator;
    private readonly RejectCollaboratorHandler _rejectCollaborator;

    public CollaboratorController(
        ApplyCollaboratorHandler applyCollaborator,
        ApproveCollaboratorHandler approveCollaborator,
        RejectCollaboratorHandler rejectCollaborator)
    {
        _applyCollaborator = applyCollaborator;
        _approveCollaborator = approveCollaborator;
        _rejectCollaborator = rejectCollaborator;
    }

    [Authorize]
    [HttpPost("apply")]
    public async Task<IActionResult> Apply()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _applyCollaborator.ExecuteAsync(userId);
        return Accepted();
    }

    [Authorize(Roles = "admin")]
    [HttpPost("requests/{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _approveCollaborator.ExecuteAsync(id, adminId);
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpPost("requests/{id}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _rejectCollaborator.ExecuteAsync(id, adminId);
        return NoContent();
    }
}

