using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

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
        var authority = _configuration["Auth:Authority"]?.TrimEnd('/');
        var clientId = _configuration["Auth:ClientId"] ?? "parking-ui";
        var redirectUri = $"{Request.Scheme}://{Request.Host}/";
        var idToken = await HttpContext.GetTokenAsync("id_token");

        foreach (var cookie in Request.Cookies.Keys)
        {
            Response.Cookies.Delete(cookie);
            Response.Cookies.Delete(cookie, new CookieOptions { Path = "/auth" });
        }

        Response.Cookies.Append(
            "force_login",
            "1",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.FromMinutes(5)
            });

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (string.IsNullOrWhiteSpace(authority))
        {
            return Redirect("/");
        }

        var logoutUrl = $"{authority}/protocol/openid-connect/logout?client_id={Uri.EscapeDataString(clientId)}";
        if (!string.IsNullOrWhiteSpace(idToken))
        {
            logoutUrl += $"&id_token_hint={Uri.EscapeDataString(idToken)}";
        }
        logoutUrl += $"&post_logout_redirect_uri={Uri.EscapeDataString(redirectUri)}";

        return Redirect(logoutUrl);
    }
}
