using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Parking.CoreApi.Pages.Account;

public sealed class LogoutModel : PageModel
{
    private readonly IConfiguration _configuration;

    public LogoutModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var authority = _configuration["Auth:Authority"];
        if (string.IsNullOrWhiteSpace(authority))
        {
            return Redirect("/");
        }

        var redirectUri = $"{Request.Scheme}://{Request.Host}/";
        var idToken = await HttpContext.GetTokenAsync("id_token");
        var url = $"{authority}/protocol/openid-connect/logout?post_logout_redirect_uri={Uri.EscapeDataString(redirectUri)}";
        if (!string.IsNullOrWhiteSpace(idToken))
        {
            url += $"&id_token_hint={Uri.EscapeDataString(idToken)}";
        }

        return Redirect(url);
    }
}
