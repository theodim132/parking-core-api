using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.CoreApi.Data;
using Parking.CoreApi.Dtos;
using Parking.CoreApi.Models;

namespace Parking.CoreApi.Controllers;

[ApiController]
[Route("api/admin/applications")]
public sealed class AdminApplicationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminApplicationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ParkingPermitApplication>>> Get([FromQuery] ApplicationStatus? status, CancellationToken ct)
    {
        var query = _db.Applications.AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var apps = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
        return Ok(apps);
    }

    [HttpPost("{id:guid}/decision")]
    public async Task<ActionResult> Decide(Guid id, [FromBody] AdminDecisionRequest request, CancellationToken ct)
    {
        if (request.Status != ApplicationStatus.Approved && request.Status != ApplicationStatus.Rejected)
        {
            return BadRequest("Status must be Approved or Rejected.");
        }

        var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (app is null)
        {
            return NotFound();
        }

        if (app.Status != ApplicationStatus.Submitted)
        {
            return BadRequest("Only submitted applications can be decided.");
        }

        app.Status = request.Status;
        app.UpdatedAt = DateTimeOffset.UtcNow;

        var decisionText = request.Status == ApplicationStatus.Approved ? "approved" : "rejected";
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? string.Empty : $" Reason: {request.Reason}";

        _db.OutboxEmails.Add(new OutboxEmail
        {
            ToEmail = app.Email,
            Subject = $"Application {decisionText}",
            Body = $"Your parking permit application {app.Id} was {decisionText}.{reason}",
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}