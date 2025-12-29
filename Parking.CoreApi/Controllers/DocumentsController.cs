using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.CoreApi.Data;
using Parking.CoreApi.Dtos;
using Parking.CoreApi.Models;
using Parking.CoreApi.Services;

namespace Parking.CoreApi.Controllers;

[ApiController]
[Route("api")]
public sealed class DocumentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IObjectStorage _storage;

    public DocumentsController(AppDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpPost("applications/{id:guid}/documents")]
    public async Task<ActionResult<DocumentUploadResponse>> Upload(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file.Length <= 0)
        {
            return BadRequest("Empty file.");
        }

        var citizenId = GetCitizenId();
        var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id && a.CitizenId == citizenId, ct);
        if (app is null)
        {
            return NotFound();
        }

        if (app.Status != ApplicationStatus.Draft)
        {
            return BadRequest("Documents can only be uploaded for draft applications.");
        }

        await using var stream = file.OpenReadStream();
        var key = await _storage.UploadAsync(stream, file.FileName, file.ContentType, ct);

        var doc = new ApplicationDocument
        {
            ApplicationId = app.Id,
            FileName = file.FileName,
            ContentType = file.ContentType ?? "application/octet-stream",
            StorageKey = key,
            SizeBytes = file.Length,
            UploadedAt = DateTimeOffset.UtcNow
        };

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync(ct);

        return Ok(new DocumentUploadResponse
        {
            Id = doc.Id,
            FileName = doc.FileName,
            StorageKey = doc.StorageKey
        });
    }

    [HttpGet("documents/{id:guid}")]
    public async Task<ActionResult> Download(Guid id, CancellationToken ct)
    {
        var doc = await _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);
        if (doc is null)
        {
            return NotFound();
        }

        var stream = await _storage.DownloadAsync(doc.StorageKey, ct);
        return File(stream, doc.ContentType, doc.FileName);
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