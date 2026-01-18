using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parking.CoreApi.Dtos;
using Parking.CoreApi.Models;
using Parking.CoreApi.Services;

namespace Parking.CoreApi.Pages.Apply;

public sealed class IndexModel : PageModel
{
    private readonly IApplicationService _service;

    public IndexModel(IApplicationService service)
    {
        _service = service;
    }

    [BindProperty]
    public ApplicationInput Input { get; set; } = new();

    public List<ParkingPermitApplication> MyApplications { get; private set; } = new();
    public string CitizenId { get; private set; } = string.Empty;
    public string? SuccessMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        CitizenId = ResolveCitizenId();
        Input.Email = User.FindFirstValue(ClaimTypes.Email) ?? Input.Email;
        Input.FullName = User.FindFirstValue("name") ?? Input.FullName;

        MyApplications = await _service.GetForCitizenAsync(CitizenId, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        CitizenId = ResolveCitizenId();

        if (!ModelState.IsValid)
        {
            MyApplications = await _service.GetForCitizenAsync(CitizenId, ct);
            return Page();
        }

        var request = new CreateApplicationRequest
        {
            FullName = Input.FullName.Trim(),
            Address = Input.Address.Trim(),
            PlateNumber = Input.PlateNumber.Trim(),
            Email = Input.Email.Trim(),
            Phone = Input.Phone.Trim()
        };

        var app = await _service.CreateAsync(CitizenId, request, ct);
        var submit = await _service.SubmitAsync(app.Id, CitizenId, ct);
        if (!submit.Success)
        {
            ModelState.AddModelError(string.Empty, submit.Error ?? "Unable to submit application.");
            MyApplications = await _service.GetForCitizenAsync(CitizenId, ct);
            return Page();
        }

        SuccessMessage = $"Application {app.Id} submitted.";
        Input = new ApplicationInput();
        MyApplications = await _service.GetForCitizenAsync(CitizenId, ct);
        return Page();
    }

    private string ResolveCitizenId()
    {
        return User.FindFirstValue("sub")
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue(ClaimTypes.Email)
               ?? User.Identity?.Name
               ?? "unknown";
    }

    public sealed class ApplicationInput
    {
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Phone { get; set; } = string.Empty;
    }
}
