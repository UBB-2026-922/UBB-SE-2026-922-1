// <copyright file="ServiceCollectionExtensions.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ServiceCollectionExtensions class.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Login;
using BankingApp.Application.Services.Notifications;
using BankingApp.Application.Services.Security;
using BankingApp.Application.Services.TeamB;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.DataAccess.Implementations;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using BankingApp.Infrastructure.Repositories.Implementations;
using BankingApp.Infrastructure.Services;
using BankingApp.Infrastructure.Services.Notifications;
using BankingApp.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        services.AddDbContext<AppDatabaseContext>(options =>
            options.UseSqlServer(connectionString)
                   .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
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
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<IBillerDataAccess, BillerDataAccess>();
        services.AddScoped<ISavedBillerDataAccess, SavedBillerDataAccess>();
        services.AddScoped<IBillerRepository, BillerRepository>();
        services.AddScoped<ITransferDataAccess, TransferDataAccess>();

        services.AddSingleton<IOtpAttemptTracker, OtpAttemptTracker>();
        services.AddSingleton<IOtpService, OtpService>(_ => new OtpService(otpSecret));

        // Team B landing zones.
        // These registrations wire the interfaces defined in the Application layer to the
        // repository implementations in the Infrastructure layer.
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IBillPaymentRepository, BillPaymentRepository>();
        services.AddScoped<IRecurringPaymentRepository, RecurringPaymentRepository>();
        services.AddScoped<IExchangeRepository, ExchangeRepository>();
        services.AddScoped<IRateAlertRepository, RateAlertRepository>();

        return services;
    }
}
