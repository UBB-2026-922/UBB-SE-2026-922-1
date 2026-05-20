using System.Globalization;
using System.Security.Claims;
using BankingApp.Contracts.Features.Authentication.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Web.DependencyInjection;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ClientAuthenticationService = BankingApp.Application.Features.Authentication.Services.IAuthenticationService;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

builder.Services.AddControllersWithViews();
builder.Services.AddWebClientServices(apiBaseUrl);
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapGet(
            "/dev/login",
            async (
                HttpContext context,
                ClientAuthenticationService authenticationService,
                IConfiguration configuration,
                string? returnUrl) =>
            {
                string? email = configuration["DevLogin:Email"];
                string? password = configuration["DevLogin:Password"];

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return Results.Problem(
                        "Dev login is not configured. Set DevLogin:Email and DevLogin:Password in appsettings.Development.json or user secrets.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }

                ErrorOr<LoginSuccessResponse> result = await authenticationService.LoginAsync(
                    new LoginRequest { Email = email, Password = password },
                    context.RequestAborted);

                if (result.IsError)
                {
                    return Results.Problem(
                        $"Dev login failed: {result.FirstError.Description}",
                        statusCode: StatusCodes.Status502BadGateway);
                }

                LoginSuccessResponse login = result.Value;

                if (login.Requires2Fa)
                {
                    return Results.Problem(
                        "Dev login cannot use an account that requires two-factor authentication.",
                        statusCode: StatusCodes.Status502BadGateway);
                }

                if (string.IsNullOrWhiteSpace(login.Token))
                {
                    return Results.Problem(
                        "Dev login failed because the API did not return an authentication token.",
                        statusCode: StatusCodes.Status502BadGateway);
                }

                string userId = login.UserId.ToString(CultureInfo.InvariantCulture);
                Claim[] claims =
                [
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, email),
                    new Claim(AuthClaimTypes.UserId, userId),
                    new Claim(AuthClaimTypes.Token, login.Token),
                    new Claim(AuthClaimTypes.SessionId, login.SessionId?.ToString(CultureInfo.InvariantCulture) ?? "0")
                ];

                ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new()
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
                };

                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    properties);

                return Results.Redirect(IsLocalReturnUrl(returnUrl) ? returnUrl! : "/");
            })
        .AllowAnonymous();

    app.MapPost(
            "/dev/logout",
            async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Results.Redirect("/");
            })
        .AllowAnonymous();
}

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
return;

static bool IsLocalReturnUrl(string? returnUrl)
{
    return !string.IsNullOrEmpty(returnUrl)
           && returnUrl[0] == '/'
           && (returnUrl.Length == 1 || (returnUrl[1] != '/' && returnUrl[1] != '\\'));
}
