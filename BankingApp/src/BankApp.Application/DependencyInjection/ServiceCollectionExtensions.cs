// <copyright file="ServiceCollectionExtensions.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ServiceCollectionExtensions class.
// </summary>

using BankApp.Application.Services.Dashboard;
using BankApp.Application.Services.Login;
using BankApp.Application.Services.PasswordRecovery;
using BankApp.Application.Services.Profile;
using BankApp.Application.Services.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace BankApp.Application.DependencyInjection;

/// <summary>
///     Provides extension methods for registering application-layer services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers application use cases and orchestration services with the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
