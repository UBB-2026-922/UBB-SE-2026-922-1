namespace BankingApp.Api.HostedServices;

using Application.Features.ForexRateAlerts.Commands;
using Application.Features.RecurringPayments.Commands;
using ErrorOr;
using Logging;
using MediatR;

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
                ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

                ErrorOr<int> recurringResult = await sender.Send(new ProcessDueRecurringPaymentsCommand(), stoppingToken);
                if (recurringResult.IsError)
                {
                    _logger.RecurringPaymentProcessingFailed(recurringResult.FirstError.Description);
                }

                ErrorOr<int> rateAlertResult = await sender.Send(new ProcessRateAlertsCommand(), stoppingToken);
                if (rateAlertResult.IsError)
                {
                    _logger.RateAlertProcessingFailed(rateAlertResult.FirstError.Description);
                }
            }
            catch (Exception exception)
            {
                _logger.BackgroundFinanceProcessingFailed(exception);
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }
}
