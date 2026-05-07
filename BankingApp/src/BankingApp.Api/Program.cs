using System.Globalization;
using BankingApp.Api.HostedServices;
using BankingApp.Api.Middleware;
using BankingApp.Application.DependencyInjection;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.DependencyInjection;
using BankingApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;

const string defaultLogFilePath = "logs/bankingapp-server-.log";
const int retainedLogFileCountLimit = 14;
const int commandLineExecutableArgumentCount = 1;
const int internalServerErrorStatusCode = StatusCodes.Status500InternalServerError;
const string applyDatabaseMigrationsConfigurationKey = "Database:ApplyMigrations";

// Configure Serilog before building the host so that startup errors are also captured.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.File(
        defaultLogFilePath,
        formatProvider: CultureInfo.InvariantCulture,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: retainedLogFileCountLimit)
    .CreateBootstrapLogger();
try
{
    Log.Information("Starting BankingApp.Api");
    string[] commandLineArguments = Environment.GetCommandLineArgs().Skip(commandLineExecutableArgumentCount).ToArray();
    WebApplicationBuilder builder = WebApplication.CreateBuilder(commandLineArguments);
    // Replace the default MEL providers with Serilog. Configuration (log levels, sinks)
    // can be further overridden via appsettings.json under the "Serilog" key.
    builder.Host.UseSerilog((context, services, configurationBuilder) =>
        configurationBuilder
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(
                context.Configuration["Logging:FilePath"] ?? defaultLogFilePath,
                formatProvider: CultureInfo.InvariantCulture,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: retainedLogFileCountLimit));
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste your JWT token here",
            });
        options.AddSecurityRequirement(_ =>
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference(referenceId: "Bearer"),
                    []
                },
            });
    });
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHostedService<FinanceBackgroundService>();
    WebApplication application = builder.Build();
    bool applyDatabaseMigrations = !bool.TryParse(
        application.Configuration[applyDatabaseMigrationsConfigurationKey],
        out bool configuredApplyDatabaseMigrations) || configuredApplyDatabaseMigrations;
    if (applyDatabaseMigrations && !application.Environment.IsEnvironment("Testing"))
    {
        using IServiceScope scope = application.Services.CreateScope();
        AppDatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<AppDatabaseContext>();
        databaseContext.Database.Migrate();
    }

    application.UseExceptionHandler(exceptionApplicationBuilder => exceptionApplicationBuilder.Run(async context =>
    {
        context.Response.StatusCode = internalServerErrorStatusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "Something went wrong." });
    }));
    if (application.Environment.IsDevelopment())
    {
        application.UseSwagger();
        application.UseSwaggerUI();
    }

    // Logs each HTTP request: method, path, status code, and duration.
    // Placed before middleware that may short-circuit the pipeline so all
    // requests are captured, including those rejected by session validation.
    application.UseSerilogRequestLogging();
    application.UseMiddleware<SessionValidationMiddleware>();
    application.MapControllers();
    application.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "BankingApp.Api terminated unexpectedly");
    throw;
}
finally
{
    // Flush and close all Serilog sinks before the process exits.
    Log.CloseAndFlush();
}