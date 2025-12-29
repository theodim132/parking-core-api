using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.CoreApi.Data;
using Parking.CoreApi.Dtos;
using Parking.CoreApi.Models;

namespace Parking.CoreApi.Controllers;

[ApiController]
[Route("api/applications")]
public sealed class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ApplicationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ParkingPermitApplication>>> GetMyApplications(CancellationToken ct)
    {
        var citizenId = GetCitizenId();
        var apps = await _db.Applications
            .AsNoTracking()
            .Where(a => a.CitizenId == citizenId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return Ok(apps);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ParkingPermitApplication>> GetById(Guid id, CancellationToken ct)
    {
        var citizenId = GetCitizenId();
        var app = await _db.Applications
            .AsNoTracking()
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id && a.CitizenId == citizenId, ct);

        return app is null ? NotFound() : Ok(app);
    }

    [HttpPost]
    public async Task<ActionResult<ParkingPermitApplication>> Create([FromBody] CreateApplicationRequest request, CancellationToken ct)
    {
        var citizenId = GetCitizenId();
        var app = new ParkingPermitApplication
        {
            CitizenId = citizenId,
            FullName = request.FullName,
            Address = request.Address,
            PlateNumber = request.PlateNumber,
            Email = request.Email,
            Phone = request.Phone,
            Status = ApplicationStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Applications.Add(app);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = app.Id }, app);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateApplicationRequest request, CancellationToken ct)
    {
        var citizenId = GetCitizenId();
        var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id && a.CitizenId == citizenId, ct);

        if (app is null)
        {
            return NotFound();
        }

        if (app.Status != ApplicationStatus.Draft)
        {
            return BadRequest("Only draft applications can be updated.");
        }

        app.FullName = request.FullName;
        app.Address = request.Address;
        app.PlateNumber = request.PlateNumber;
        app.Email = request.Email;
        app.Phone = request.Phone;
        app.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult> Submit(Guid id, CancellationToken ct)
    {
        var citizenId = GetCitizenId();
        var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id && a.CitizenId == citizenId, ct);

        if (app is null)
        {
            return NotFound();
        }

        if (app.Status != ApplicationStatus.Draft)
        {
            return BadRequest("Only draft applications can be submitted.");
        }

        app.Status = ApplicationStatus.Submitted;
        app.UpdatedAt = DateTimeOffset.UtcNow;

        _db.OutboxEmails.Add(new OutboxEmail
        {
            ToEmail = app.Email,
            Subject = "Application submitted",
            Body = $"Your parking permit application {app.Id} was submitted.",
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private string GetCitizenId()
    {
        var claim = User.FindFirst("sub")?.Value;
        if (!string.IsNullOrWhiteSpace(claim))
        {
            return claim;
        }

        if (Request.Headers.TryGetValue("X-Citizen-Id", out var header) && !string.IsNullOrWhiteSpace(header))
        {
            return header!;
        }

        throw new InvalidOperationException("Missing citizen identity.");
    }
}