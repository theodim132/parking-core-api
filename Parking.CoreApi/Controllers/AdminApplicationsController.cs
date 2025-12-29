using Microsoft.AspNetCore.Mvc;
using Parking.CoreApi.Dtos;
using Parking.CoreApi.Models;
using Parking.CoreApi.Services;

namespace Parking.CoreApi.Controllers;

[ApiController]
[Route("api/admin/applications")]
public sealed class AdminApplicationsController : ControllerBase
{
    private readonly IApplicationService _service;

    public AdminApplicationsController(IApplicationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ParkingPermitApplication>>> Get([FromQuery] ApplicationStatus? status, CancellationToken ct)
    {
        var apps = await _service.GetAdminListAsync(status, ct);
        return Ok(apps);
    }

    [HttpPost("{id:guid}/decision")]
    public async Task<ActionResult> Decide(Guid id, [FromBody] AdminDecisionRequest request, CancellationToken ct)
    {
        var result = await _service.DecideAsync(id, request, ct);
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
}
