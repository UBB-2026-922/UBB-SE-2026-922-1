namespace BankingApp.Application.Common.Behaviors;

using MediatR;
using Microsoft.Extensions.Logging;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
        }

        try
        {
            TResponse response = await next();
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
