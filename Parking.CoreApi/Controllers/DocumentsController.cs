using Microsoft.AspNetCore.Mvc;
using Parking.CoreApi.Dtos;
using Parking.CoreApi.Services;

namespace Parking.CoreApi.Controllers;

[ApiController]
[Route("api")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentsController(IDocumentService service)
    {
        _service = service;
    }

    [HttpPost("applications/{id:guid}/documents")]
    public async Task<ActionResult<DocumentUploadResponse>> Upload(Guid id, IFormFile file, CancellationToken ct)
    {
        var citizenId = GetCitizenId();
        var result = await _service.UploadAsync(id, citizenId, file, ct);
        if (result.Success)
        {
            return Ok(result.Value);
        }

        return result.Code switch
        {
            "NotFound" => NotFound(result.Error),
            "InvalidState" => BadRequest(result.Error),
            "Validation" => BadRequest(result.Error),
            _ => BadRequest(result.Error)
        };
    }

    [HttpGet("documents/{id:guid}")]
    public async Task<ActionResult> Download(Guid id, CancellationToken ct)
    {
        var result = await _service.DownloadAsync(id, ct);
        if (!result.Success || result.Value is null)
        {
            return result.Code == "NotFound" ? NotFound(result.Error) : BadRequest(result.Error);
        }

        return File(result.Value.Stream, result.Value.ContentType, result.Value.FileName);
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
