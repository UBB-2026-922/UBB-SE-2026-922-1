using System;
using Microsoft.Extensions.Logging;

namespace BankingApp.Api.Logging;

internal static partial class ApiLogMessages
{
    [LoggerMessage(EventId = 3000, Level = LogLevel.Warning, Message = "Recurring payment processing failed: {Error}")]
    internal static partial void RecurringPaymentProcessingFailed(this ILogger logger, string error);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Warning, Message = "Rate-alert processing failed: {Error}")]
    internal static partial void RateAlertProcessingFailed(this ILogger logger, string error);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Error, Message = "Background finance processing failed.")]
    internal static partial void BackgroundFinanceProcessingFailed(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Warning, Message = "Token validation failed [{Code}]: {Description}")]
    internal static partial void TokenValidationFailed(this ILogger logger, string code, string description);

    [LoggerMessage(EventId = 3004, Level = LogLevel.Warning, Message = "Session lookup failed [{Code}]: {Description}")]
    internal static partial void SessionLookupFailed(this ILogger logger, string code, string description);
}
