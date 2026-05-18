namespace BankingApp.Application.Common.Behaviors;

using MediatR;
using Microsoft.Extensions.Logging;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly Action<ILogger, string, Exception?> _handlingRequest =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(HandlingRequest)),
            "Handling {RequestName}");

    private static readonly Action<ILogger, string, Exception?> _handledRequest =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(HandledRequest)),
            "Handled {RequestName}");

    private static readonly Action<ILogger, string, Exception?> _requestHandlingFailed =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(3, nameof(RequestHandlingFailed)),
            "Error handling {RequestName}");

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            HandlingRequest(logger, typeof(TRequest).Name);
        }

        try
        {
            TResponse response = await next();
            if (logger.IsEnabled(LogLevel.Information))
            {
                HandledRequest(logger, typeof(TRequest).Name);
            }

            return response;
        }
        catch (Exception ex)
        {
            RequestHandlingFailed(logger, typeof(TRequest).Name, ex);
            throw;
        }
    }

    private static void HandlingRequest(ILogger logger, string requestName) =>
        _handlingRequest(logger, requestName, null);

    private static void HandledRequest(ILogger logger, string requestName) =>
        _handledRequest(logger, requestName, null);

    private static void RequestHandlingFailed(ILogger logger, string requestName, Exception exception) =>
        _requestHandlingFailed(logger, requestName, exception);
}
