// <copyright file="FinanceBackgroundService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the FinanceBackgroundService class.
// </summary>

using BankingApp.Application.Services.RecurringPayments;
using BankingApp.Application.Services.TeamB;
using ErrorOr;

namespace BankingApp.Api.HostedServices;

/// <summary>
///     Periodically processes due recurring payments and pending rate alerts.
/// </summary>
public class FinanceBackgroundService : BackgroundService
{
    private static readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FinanceBackgroundService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FinanceBackgroundService" /> class.
    /// </summary>
    /// <param name="serviceProvider">The root service provider.</param>
    /// <param name="logger">The logger.</param>
    public FinanceBackgroundService(IServiceProvider serviceProvider, ILogger<FinanceBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                IRecurringPaymentProcessingService recurringPaymentProcessingService = scope.ServiceProvider.GetRequiredService<IRecurringPaymentProcessingService>();
                IRateAlertService rateAlertService = scope.ServiceProvider.GetRequiredService<IRateAlertService>();

                var recurringResult = await recurringPaymentProcessingService.ProcessDuePaymentsAsync(stoppingToken);
                if (recurringResult.IsError)
                {
                    _logger.LogWarning("Recurring payment processing failed: {Error}", recurringResult.FirstError.Description);
                }

                var rateAlertResult = rateAlertService.ProcessAlerts();
                if (rateAlertResult.IsError)
                {
                    _logger.LogWarning("Rate-alert processing failed: {Error}", rateAlertResult.FirstError.Description);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Background finance processing failed.");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }
}
