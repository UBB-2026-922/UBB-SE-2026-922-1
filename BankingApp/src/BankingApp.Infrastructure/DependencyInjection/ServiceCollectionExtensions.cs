// <copyright file="ServiceCollectionExtensions.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ServiceCollectionExtensions class.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Login;
using BankingApp.Application.Services.Notifications;
using BankingApp.Application.Services.Security;
using BankingApp.Application.Utilities;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.DataAccess.Implementations;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using BankingApp.Infrastructure.Repositories.Implementations;
using BankingApp.Infrastructure.Services;
using BankingApp.Infrastructure.Services.Notifications;
using BankingApp.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApp.Infrastructure.DependencyInjection;

/// <summary>
///     Provides extension methods for registering infrastructure services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly Lock _typeHandlerLock = new();

    /// <summary>
    ///     Registers all infrastructure services, data access components, and repositories with the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration used to resolve connection strings and secrets.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the <c>BankingAppDb</c> connection string,
    ///     <c>Jwt:Secret</c> or <c>Otp:Secret</c> configuration value is missing.
    /// </exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("BankingAppDb")
                                  ?? throw new InvalidOperationException(
                                      "Configuration value 'ConnectionStrings:BankingAppDb' is missing.");
        string jwtSecret = configuration["Jwt:Secret"]
                           ?? throw new InvalidOperationException("Configuration value 'Jwt:Secret' is missing.");
        string otpSecret = configuration["Otp:Secret"]
                           ?? throw new InvalidOperationException("Configuration value 'Otp:Secret' is missing.");

        services.AddDbContext<AppDatabaseContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUserDataAccess, UserDataAccess>();
        services.AddScoped<ISessionDataAccess, SessionDataAccess>();
        services.AddScoped<IOAuthLinkDataAccess, OAuthLinkDataAccess>();
        services.AddScoped<IPasswordResetTokenDataAccess, PasswordResetTokenDataAccess>();
        services.AddScoped<INotificationPreferenceDataAccess, NotificationPreferenceDataAccess>();
        services.AddScoped<IAccountDataAccess, AccountDataAccess>();
        services.AddScoped<ICardDataAccess, CardDataAccess>();
        services.AddScoped<ITransactionDataAccess, TransactionDataAccess>();
        services.AddScoped<INotificationDataAccess, NotificationDataAccess>();
        services.AddScoped<IHashService, HashService>();
        services.AddScoped<IJsonWebTokenService>(_ => new JsonWebTokenService(jwtSecret));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IRecurringPaymentRepository, RecurringPaymentRepository>();
        services.AddSingleton<ISystemClock, SystemClock>();

        services.AddSingleton<IOtpAttemptTracker, OtpAttemptTracker>();
        services.AddSingleton<IOtpService, OtpService>(_ => new OtpService(otpSecret));
        return services;
    }
}