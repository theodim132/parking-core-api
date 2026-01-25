using System.IO;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Parking.CoreApi.Data;
using Parking.CoreApi.Repositories;
using Parking.CoreApi.Services;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("CitizenId", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Citizen-Id",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Provide a citizen id for demo/testing."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "CitizenId"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Apply");
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.AddSingleton<IObjectStorage>(sp =>
{
    var options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
    if (string.Equals(options.Provider, "S3", StringComparison.OrdinalIgnoreCase))
    {
        return new S3ObjectStorage(options);
    }

    return new LocalObjectStorage(options);
});
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Smart";
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddPolicyScheme("Smart", "Smart", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var header = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(header) &&
                header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return JwtBearerDefaults.AuthenticationScheme;
            }

            return CookieAuthenticationDefaults.AuthenticationScheme;
        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        var authority = builder.Configuration["Auth:Authority"];
        options.Authority = authority;
        options.ClientId = builder.Configuration["Auth:ClientId"] ?? "parking-ui";
        options.ClientSecret = builder.Configuration["Auth:ClientSecret"];
        options.ResponseType = "code";
        options.UsePkce = true;
        options.ResponseMode = "query";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool?>("Auth:RequireHttpsMetadata") ?? true;
        options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
        options.CallbackPath = "/signin-oidc";
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("roles");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = ClaimTypes.Role
        };
        options.Events.OnRedirectToIdentityProvider = context =>
        {
            context.ProtocolMessage.Parameters.Remove("request_uri");
            return Task.CompletedTask;
        };
        options.Events.OnTokenValidated = context =>
        {
            var identity = (ClaimsIdentity?)context.Principal?.Identity;
            if (identity == null)
            {
                return Task.CompletedTask;
            }

            string? token = null;
            if (!string.IsNullOrWhiteSpace(context.TokenEndpointResponse?.AccessToken))
            {
                token = context.TokenEndpointResponse!.AccessToken;
            }
            else if (!string.IsNullOrWhiteSpace(context.TokenEndpointResponse?.IdToken))
            {
                token = context.TokenEndpointResponse!.IdToken;
            }
            else if (context.SecurityToken is JwtSecurityToken jwtToken)
            {
                token = jwtToken.RawData;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return Task.CompletedTask;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var realmAccess = jwt.Claims.FirstOrDefault(c => c.Type == "realm_access")?.Value;
            if (string.IsNullOrWhiteSpace(realmAccess))
            {
                return Task.CompletedTask;
            }

            using var doc = JsonDocument.Parse(realmAccess);
            if (doc.RootElement.TryGetProperty("roles", out var roles) &&
                roles.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in roles.EnumerateArray())
                {
                    var value = role.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, value));
                    }
                }
            }
            return Task.CompletedTask;
        };
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
        options.RequireHttpsMetadata = false;
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
});

var app = builder.Build();

var migrateOnly = builder.Configuration.GetValue<bool>("MIGRATE_ONLY");
var migrateAtStartup = builder.Configuration.GetValue<bool>("MIGRATE_AT_STARTUP");

if (migrateOnly || migrateAtStartup)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    if (migrateOnly)
    {
        return;
    }
}

app.UseForwardedHeaders();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
