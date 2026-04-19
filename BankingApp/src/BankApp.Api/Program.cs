// <copyright file="Program.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the Program class.
// </summary>

using BankApp.Api.Middleware;
using BankApp.Application.DependencyInjection;
using BankApp.Infrastructure.DependencyInjection;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;

const string defaultLogFilePath = "logs/bankapp-server-.log";
const int retainedLogFileCountLimit = 14;
const int commandLineExecutableArgumentCount = 1;
const int internalServerErrorStatusCode = StatusCodes.Status500InternalServerError;
// Configure Serilog before building the host so that startup errors are also captured.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        defaultLogFilePath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: retainedLogFileCountLimit)
    .CreateBootstrapLogger();
try
{
    Log.Information("Starting BankApp.Api");
    string[] commandLineArguments = Environment.GetCommandLineArgs().Skip(commandLineExecutableArgumentCount).ToArray();
    WebApplicationBuilder builder = WebApplication.CreateBuilder(commandLineArguments);
    // Replace the default MEL providers with Serilog. Configuration (log levels, sinks)
    // can be further overridden via appsettings.json under the "Serilog" key.
    builder.Host.UseSerilog((context, services, configurationBuilder) =>
        configurationBuilder
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                context.Configuration["Logging:FilePath"] ?? defaultLogFilePath,
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
                    new OpenApiSecuritySchemeReference("Bearer", null, null),
                    []
                },
            });
    });
    // Allow the client to connect
    builder.Services.AddCors(corsOptions => corsOptions.AddDefaultPolicy(corsPolicy =>
        corsPolicy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    WebApplication application = builder.Build();
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

    application.UseCors();
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
    Log.Fatal(exception, "BankApp.Api terminated unexpectedly");
}
finally
{
    // Flush and close all Serilog sinks before the process exits.
    Log.CloseAndFlush();
}

/// <summary>
///     Exposes the auto-generated Program class so integration tests can reference it
///     via <c>WebApplicationFactory&lt;Program&gt;</c>.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program
{
}
