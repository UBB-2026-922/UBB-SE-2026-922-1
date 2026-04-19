// <copyright file="ServiceCollectionExtensions.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ServiceCollectionExtensions class.
// </summary>

using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Login;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.Security;
using BankApp.Domain.Enums;
using BankApp.Infrastructure.DataAccess;
using BankApp.Infrastructure.DataAccess.Implementations;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Infrastructure.DataAccess.TypeHandlers;
using BankApp.Infrastructure.Repositories.Implementations;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Services.Notifications;
using BankApp.Infrastructure.Services.Security;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankApp.Infrastructure.DependencyInjection;

/// <summary>
///     Provides extension methods for registering infrastructure services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly Lock _typeHandlerLock = new();
    private static bool _typeHandlersRegistered;

    /// <summary>
    ///     Registers all infrastructure services, data access components, and repositories with the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration used to resolve connection strings and secrets.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the <c>DefaultConnection</c> connection string or <c>Jwt:Secret</c> configuration value is missing.
    /// </exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterTypeHandlers();
        string connectionString = configuration.GetConnectionString("DefaultConnection")
                                  ?? throw new InvalidOperationException(
                                      "Connection string 'DefaultConnection' is missing.");
        string jwtSecret = configuration["Jwt:Secret"]
                           ?? throw new InvalidOperationException("Configuration value 'Jwt:Secret' is missing.");
        services.AddScoped<AppDatabaseContext>(_ => new AppDatabaseContext(connectionString));
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
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, EmailService>();
        // Singleton: OTP attempt counters must survive individual request scopes.
        services.AddSingleton<IOtpAttemptTracker, OtpAttemptTracker>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        return services;
    }

    private static void RegisterTypeHandlers()
    {
        lock (_typeHandlerLock)
        {
            if (_typeHandlersRegistered)
            {
                return;
            }
        }

        lock (_typeHandlerLock)
        {
            if (_typeHandlersRegistered)
            {
                return;
            }

            SqlMapper.AddTypeHandler(new EnumTypeHandler<TransactionDirection>());
            SqlMapper.AddTypeHandler(new EnumTypeHandler<TransactionStatus>());
            SqlMapper.AddTypeHandler(new EnumTypeHandler<CardType>());
            SqlMapper.AddTypeHandler(new EnumTypeHandler<CardStatus>());
            SqlMapper.AddTypeHandler(new EnumTypeHandler<TwoFactorMethod>());
            SqlMapper.AddTypeHandler(new EnumTypeHandler<AccountType>());
            SqlMapper.AddTypeHandler(new EnumTypeHandler<AccountStatus>());
            SqlMapper.AddTypeHandler(new NotificationTypeHandler());
            _typeHandlersRegistered = true;
        }
    }
}
