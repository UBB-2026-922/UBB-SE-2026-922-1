// <copyright file="DomainEnumMapper.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the DomainEnumMapper class.
// </summary>

using ApplicationCardStatus = BankingApp.Application.Enums.CardStatus;
using ApplicationCardType = BankingApp.Application.Enums.CardType;
using ApplicationNotificationType = BankingApp.Application.Enums.NotificationType;
using ApplicationTransactionDirection = BankingApp.Application.Enums.TransactionDirection;
using ApplicationTransactionStatus = BankingApp.Application.Enums.TransactionStatus;
using ApplicationTwoFactorMethod = BankingApp.Application.Enums.TwoFactorMethod;
using DomainCardStatus = BankingApp.Domain.Enums.CardStatus;
using DomainCardType = BankingApp.Domain.Enums.CardType;
using DomainNotificationType = BankingApp.Domain.Enums.NotificationType;
using DomainTransactionDirection = BankingApp.Domain.Enums.TransactionDirection;
using DomainTransactionStatus = BankingApp.Domain.Enums.TransactionStatus;
using DomainTwoFactorMethod = BankingApp.Domain.Enums.TwoFactorMethod;

namespace BankingApp.Application.Mapping;

/// <summary>
///     Converts between domain enums and application-facing contract enums.
/// </summary>
internal static class DomainEnumMapper
{
    /// <summary>
    ///     Converts a domain card type to its application contract equivalent.
    /// </summary>
    /// <param name="value">The domain card type.</param>
    /// <returns>The application-facing card type.</returns>
    public static ApplicationCardType ToApplication(DomainCardType value)
    {
        return value switch
        {
            DomainCardType.Debit => ApplicationCardType.Debit,
            DomainCardType.Credit => ApplicationCardType.Credit,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    /// <summary>
    ///     Converts a domain card status to its application contract equivalent.
    /// </summary>
    /// <param name="value">The domain card status.</param>
    /// <returns>The application-facing card status.</returns>
    public static ApplicationCardStatus ToApplication(DomainCardStatus value)
    {
        return value switch
        {
            DomainCardStatus.Active => ApplicationCardStatus.Active,
            DomainCardStatus.Frozen => ApplicationCardStatus.Frozen,
            DomainCardStatus.Cancelled => ApplicationCardStatus.Cancelled,
            DomainCardStatus.Expired => ApplicationCardStatus.Expired,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    /// <summary>
    ///     Converts a domain transaction direction to its application contract equivalent.
    /// </summary>
    /// <param name="value">The domain transaction direction.</param>
    /// <returns>The application-facing transaction direction.</returns>
    public static ApplicationTransactionDirection ToApplication(DomainTransactionDirection value)
    {
        return value switch
        {
            DomainTransactionDirection.In => ApplicationTransactionDirection.In,
            DomainTransactionDirection.Out => ApplicationTransactionDirection.Out,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    /// <summary>
    ///     Converts a domain transaction status to its application contract equivalent.
    /// </summary>
    /// <param name="value">The domain transaction status.</param>
    /// <returns>The application-facing transaction status.</returns>
    public static ApplicationTransactionStatus ToApplication(DomainTransactionStatus value)
    {
        return value switch
        {
            DomainTransactionStatus.Pending => ApplicationTransactionStatus.Pending,
            DomainTransactionStatus.Completed => ApplicationTransactionStatus.Completed,
            DomainTransactionStatus.Failed => ApplicationTransactionStatus.Failed,
            DomainTransactionStatus.Cancelled => ApplicationTransactionStatus.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    /// <summary>
    ///     Converts a domain two-factor method to its application contract equivalent.
    /// </summary>
    /// <param name="value">The domain two-factor method.</param>
    /// <returns>The application-facing two-factor method.</returns>
    public static ApplicationTwoFactorMethod ToApplication(DomainTwoFactorMethod value)
    {
        return value switch
        {
            DomainTwoFactorMethod.Email => ApplicationTwoFactorMethod.Email,
            DomainTwoFactorMethod.Phone => ApplicationTwoFactorMethod.Phone,
            DomainTwoFactorMethod.Authenticator => ApplicationTwoFactorMethod.Authenticator,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    /// <summary>
    ///     Converts an optional domain two-factor method to its application contract equivalent.
    /// </summary>
    /// <param name="value">The optional domain two-factor method.</param>
    /// <returns>The optional application-facing two-factor method.</returns>
    public static ApplicationTwoFactorMethod? ToApplication(DomainTwoFactorMethod? value)
    {
        return value is null ? null : ToApplication(value.Value);
    }

    /// <summary>
    ///     Converts a domain notification category to its application contract equivalent.
    /// </summary>
    /// <param name="value">The domain notification category.</param>
    /// <returns>The application-facing notification category.</returns>
    public static ApplicationNotificationType ToApplication(DomainNotificationType value)
    {
        return value switch
        {
            DomainNotificationType.Payment => ApplicationNotificationType.Payment,
            DomainNotificationType.InboundTransfer => ApplicationNotificationType.InboundTransfer,
            DomainNotificationType.OutboundTransfer => ApplicationNotificationType.OutboundTransfer,
            DomainNotificationType.LowBalance => ApplicationNotificationType.LowBalance,
            DomainNotificationType.DuePayment => ApplicationNotificationType.DuePayment,
            DomainNotificationType.SuspiciousActivity => ApplicationNotificationType.SuspiciousActivity,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    /// <summary>
    ///     Converts an application two-factor method to its domain equivalent.
    /// </summary>
    /// <param name="value">The application-facing two-factor method.</param>
    /// <returns>The domain two-factor method.</returns>
    public static DomainTwoFactorMethod ToDomain(ApplicationTwoFactorMethod value)
    {
        return value switch
        {
            ApplicationTwoFactorMethod.Email => DomainTwoFactorMethod.Email,
            ApplicationTwoFactorMethod.Phone => DomainTwoFactorMethod.Phone,
            ApplicationTwoFactorMethod.Authenticator => DomainTwoFactorMethod.Authenticator,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    /// <summary>
    ///     Converts an application notification category to its domain equivalent.
    /// </summary>
    /// <param name="value">The application-facing notification category.</param>
    /// <returns>The domain notification category.</returns>
    public static DomainNotificationType ToDomain(ApplicationNotificationType value)
    {
        return value switch
        {
            ApplicationNotificationType.Payment => DomainNotificationType.Payment,
            ApplicationNotificationType.InboundTransfer => DomainNotificationType.InboundTransfer,
            ApplicationNotificationType.OutboundTransfer => DomainNotificationType.OutboundTransfer,
            ApplicationNotificationType.LowBalance => DomainNotificationType.LowBalance,
            ApplicationNotificationType.DuePayment => DomainNotificationType.DuePayment,
            ApplicationNotificationType.SuspiciousActivity => DomainNotificationType.SuspiciousActivity,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }
}
