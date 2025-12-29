using Microsoft.AspNetCore.Mvc;
using Parking.CoreApi.Dtos;
using Parking.CoreApi.Models;
using Parking.CoreApi.Services;

namespace Parking.CoreApi.Controllers;

[ApiController]
[Route("api/applications")]
public sealed class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _service;

    public ApplicationsController(IApplicationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ParkingPermitApplication>>> GetMyApplications(CancellationToken ct)
    {
        var citizenId = GetCitizenId();
        var apps = await _service.GetForCitizenAsync(citizenId, ct);
        return Ok(apps);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ParkingPermitApplication>> GetById(Guid id, CancellationToken ct)
    {
        var citizenId = GetCitizenId();
        var app = await _service.GetByIdForCitizenAsync(id, citizenId, ct);

        return app is null ? NotFound() : Ok(app);
    }

    [HttpPost]
    public async Task<ActionResult<ParkingPermitApplication>> Create([FromBody] CreateApplicationRequest request, CancellationToken ct)
    {
        var citizenId = GetCitizenId();
        var app = await _service.CreateAsync(citizenId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = app.Id }, app);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateApplicationRequest request, CancellationToken ct)
    {
        var citizenId = GetCitizenId();
        var result = await _service.UpdateAsync(id, citizenId, request, ct);
        return MapResult(result);
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult> Submit(Guid id, CancellationToken ct)
    {
        var citizenId = GetCitizenId();
        var result = await _service.SubmitAsync(id, citizenId, ct);
        return MapResult(result);
    }

    private ActionResult MapResult(ServiceResult result)
    {
        if (result.Success)
        {
            return NoContent();
        }

        return result.Code switch
        {
            "NotFound" => NotFound(result.Error),
            "InvalidState" => BadRequest(result.Error),
            "Validation" => BadRequest(result.Error),
            _ => BadRequest(result.Error)
        };
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
