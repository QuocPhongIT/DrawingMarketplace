using DrawingMarketplace.Application.Features.Collaborators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DrawingMarketplace.Api.Controllers;

[ApiController]
[Route("api/collaborators")]
public class CollaboratorController : ControllerBase
{
    private readonly ApplyCollaboratorHandler _apply;
    private readonly ApproveCollaboratorHandler _approve;
    private readonly RejectCollaboratorHandler _reject;

    public CollaboratorController(
        ApplyCollaboratorHandler apply,
        ApproveCollaboratorHandler approve,
        RejectCollaboratorHandler reject)
    {
        _apply = apply;
        _approve = approve;
        _reject = reject;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Apply()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _apply.ExecuteAsync(userId);
        return Accepted();
    }

    [Authorize(Roles = "admin")]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] string status)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (status == "approved")
            await _approve.ExecuteAsync(id, adminId);
        else if (status == "rejected")
            await _reject.ExecuteAsync(id, adminId);
        else
            return BadRequest();

        return NoContent();
    }
}
