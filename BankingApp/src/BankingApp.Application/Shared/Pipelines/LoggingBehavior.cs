namespace BankingApp.Application.Shared.Pipelines;

using MediatR;
using Microsoft.Extensions.Logging;

file static class LoggingMessages
{
    public static readonly Action<ILogger, string, Exception?> HandlingRequest =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "HandlingRequest"), "Handling {RequestName}");

    public static readonly Action<ILogger, string, Exception?> HandledRequest =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(2, "HandledRequest"), "Handled {RequestName}");

    public static readonly Action<ILogger, string, Exception?> RequestHandlingFailed =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(3, "RequestHandlingFailed"), "Error handling {RequestName}");
}

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
            LoggingMessages.HandlingRequest(logger, typeof(TRequest).Name, null);
        }

        try
        {
            TResponse response = await next(cancellationToken);
            if (logger.IsEnabled(LogLevel.Information))
            {
                LoggingMessages.HandledRequest(logger, typeof(TRequest).Name, null);
            }

            return response;
        }
        catch (Exception ex)
        {
            LoggingMessages.RequestHandlingFailed(logger, typeof(TRequest).Name, ex);
            throw;
        }
    }
}
