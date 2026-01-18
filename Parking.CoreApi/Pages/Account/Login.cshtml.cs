using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Parking.CoreApi.Pages.Account;

public sealed class LoginModel : PageModel
{
    public IActionResult OnGet(string? returnUrl = "/apply")
    {
        var redirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/apply" : returnUrl;
        var props = new AuthenticationProperties { RedirectUri = redirectUri };
        return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
    }
}
