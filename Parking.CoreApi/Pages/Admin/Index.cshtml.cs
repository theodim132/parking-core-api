using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parking.CoreApi.Dtos;
using Parking.CoreApi.Models;
using Parking.CoreApi.Services;

namespace Parking.CoreApi.Pages.Admin;

public sealed class IndexModel : PageModel
{
    private readonly IApplicationService _service;

    public IndexModel(IApplicationService service)
    {
        _service = service;
    }

    public List<ParkingPermitApplication> Applications { get; private set; } = new();
    public List<ApplicationStatus> StatusOptions { get; } = Enum.GetValues<ApplicationStatus>().ToList();
    public ApplicationStatus? SelectedStatus { get; private set; }

    public async Task OnGetAsync(ApplicationStatus? status, CancellationToken ct)
    {
        SelectedStatus = status;
        Applications = await _service.GetAdminListAsync(status, ct);
    }

    public async Task<IActionResult> OnPostAsync(Guid id, ApplicationStatus decision, CancellationToken ct)
    {
        var request = new AdminDecisionRequest { Status = decision };
        var result = await _service.DecideAsync(id, request, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error");
        }

        return RedirectToPage(new { status = SelectedStatus });
    }
}