using System.Globalization;
using System.Net.Http.Json;
using System.Security.Claims;
using BankingApp.Contracts.Http;
using BankingApp.Web.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
                IHttpClientFactory httpClientFactory,
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

                HttpClient api = httpClientFactory.CreateClient(BankingApp.Contracts.Http.HttpClientNames.Api);
                HttpResponseMessage response = await api.PostAsJsonAsync(
                    ApiEndpoints.Auth.LoginFull,
                    new { Email = email, Password = password },
                    context.RequestAborted);

                if (!response.IsSuccessStatusCode)
                {
                    string apiError = await response.Content.ReadAsStringAsync(context.RequestAborted);

                    return Results.Problem(
                        $"Dev login failed with {(int)response.StatusCode}: {apiError}",
                        statusCode: StatusCodes.Status502BadGateway);
                }

                DevLoginResponse? login = await response.Content.ReadFromJsonAsync<DevLoginResponse>(
                    cancellationToken: context.RequestAborted);

                if (login is null)
                {
                    return Results.Problem(
                        "Dev login failed because the API returned an empty response.",
                        statusCode: StatusCodes.Status502BadGateway);
                }

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
                    new Claim(AuthClaimTypes.Token, login.Token)
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

internal sealed record DevLoginResponse(int UserId, string? Token, bool Requires2Fa);
