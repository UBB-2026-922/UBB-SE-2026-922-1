// <copyright file="ServiceCollectionExtensions.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ServiceCollectionExtensions class.
// </summary>

using BankingApp.Application.Services.Beneficiary;
using BankingApp.Application.Services.Billers;
using BankingApp.Application.Services.Dashboard;
using BankingApp.Application.Services.Login;
using BankingApp.Application.Services.PasswordRecovery;
using BankingApp.Application.Services.Profile;
using BankingApp.Application.Services.RecurringPayments;
using BankingApp.Application.Services.Registration;
using BankingApp.Application.Services.Transfers;
using Microsoft.Extensions.DependencyInjection;
using ExchangeServiceContract = BankingApp.Application.Services.TeamB.IExchangeService;
using ExchangeServiceImplementation = BankingApp.Application.Services.TeamB.ExchangeService;
using RateAlertServiceContract = BankingApp.Application.Services.TeamB.IRateAlertService;
using RateAlertServiceImplementation = BankingApp.Application.Services.TeamB.RateAlertService;

namespace BankingApp.Application.DependencyInjection;

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
        services.AddScoped<IBillerService, BillerService>();
        services.AddScoped<BankingApp.Application.Services.Transfers.ITransferService, TransferService>();
        services.AddScoped<BankingApp.Application.Services.Beneficiary.IBeneficiaryService, BeneficiaryService>();
        services.AddScoped<BankingApp.Application.Services.RecurringPayments.IRecurringPaymentService, RecurringPaymentService>();
        services.AddScoped<IRecurringPaymentProcessingService, RecurringPaymentProcessingService>();
        services.AddScoped<ExchangeServiceContract, ExchangeServiceImplementation>();
        services.AddScoped<RateAlertServiceContract, RateAlertServiceImplementation>();
        return services;
    }
}
