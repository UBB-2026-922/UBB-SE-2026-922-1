using BankingApp.Web.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<IBeneficiaryService, BeneficiaryService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5024");
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

                HttpClient api = httpClientFactory.CreateClient("BankingAppApi");
                HttpResponseMessage response = await api.PostAsJsonAsync(
                    "api/auth/login",
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
                    new Claim("userId", userId),
                    new Claim("token", login.Token)
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
