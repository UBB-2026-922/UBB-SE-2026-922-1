// <copyright file="DashboardService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the DashboardService class.
// </summary>

using BankApp.Application.DataTransferObjects.Dashboard;
using BankApp.Application.Mapping;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Domain.Entities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Application.Services.Dashboard;

/// <summary>
///     Provides aggregated dashboard data for users.
/// </summary>
public class DashboardService : IDashboardService
{
    private const int DefaultRecentTransactionLimit = 5;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ILogger<DashboardService> _logger;
    private readonly IUserRepository _userRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DashboardService" /> class.
    /// </summary>
    /// <param name="dashboardRepository">The dashboard repository.</param>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="logger">The _logger.</param>
    /// <returns>The result of the operation.</returns>
    public DashboardService(
        IDashboardRepository dashboardRepository,
        IUserRepository userRepository,
        ILogger<DashboardService> logger)
    {
        _dashboardRepository = dashboardRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<DashboardResponse> GetDashboardData(int userId)
    {
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.LogWarning("Dashboard fetch failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<Card>> cardsResult = _dashboardRepository.GetCardsByUser(userId);
        ErrorOr<int> notifCountResult = _dashboardRepository.GetUnreadNotificationCount(userId);
        if (cardsResult.IsError)
        {
            _logger.LogError(
                "Failed to fetch cards for user {UserId}: {Error}",
                userId,
                cardsResult.FirstError.Description);
        }

        if (notifCountResult.IsError)
        {
            _logger.LogError(
                "Failed to fetch notification count for user {UserId}: {Error}",
                userId,
                notifCountResult.FirstError.Description);
        }

        var allTransactions = new List<Transaction>();
        ErrorOr<List<Account>> accountsResult = _dashboardRepository.GetAccountsByUser(userId);
        var accountsById = new Dictionary<int, Account>();
        if (accountsResult.IsError)
        {
            _logger.LogError(
                "Failed to fetch accounts for user {UserId}: {Error}",
                userId,
                accountsResult.FirstError.Description);
        }
        else
        {
            accountsById = accountsResult.Value.ToDictionary(account => account.Id);
            foreach (Account account in accountsResult.Value)
            {
                ErrorOr<List<Transaction>> transactionsResult =
                    _dashboardRepository.GetRecentTransactions(account.Id, DefaultRecentTransactionLimit);
                if (transactionsResult.IsError)
                {
                    _logger.LogError(
                        "Failed to fetch transactions for account {AccountId}: {Error}",
                        account.Id,
                        transactionsResult.FirstError.Description);
                    continue;
                }

                allTransactions.AddRange(transactionsResult.Value);
            }

            allTransactions = allTransactions
                .OrderByDescending(transaction => transaction.CreatedAt)
                .Take(DefaultRecentTransactionLimit)
                .ToList();
        }

        return new DashboardResponse
        {
            CurrentUser = new UserSummaryDataTransferObject
            {
                FullName = userResult.Value.FullName,
                Email = userResult.Value.Email,
                PhoneNumber = userResult.Value.PhoneNumber,
                Is2FaEnabled = userResult.Value.Is2FaEnabled,
            },
            Cards = cardsResult.IsError
                ? new List<CardDataTransferObject>()
                : cardsResult.Value
                    .Select(card => new CardDataTransferObject
                    {
                        Id = card.Id,
                        CardNumber = card.GetMaskedNumber(),
                        CardholderName = card.CardholderName,
                        CardType = DomainEnumMapper.ToApplication(card.CardType),
                        CardBrand = card.CardBrand,
                        ExpiryDate = card.ExpiryDate,
                        Status = DomainEnumMapper.ToApplication(card.Status),
                        IsContactlessEnabled = card.IsContactlessEnabled,
                        IsOnlineEnabled = card.IsOnlineEnabled,
                        AccountName = accountsById.TryGetValue(card.AccountId, out Account? account)
                            ? account.AccountName
                            : null,
                        AccountBalance = account?.Balance,
                    })
                    .ToList(),
            RecentTransactions = allTransactions
                .Select(transaction => new TransactionDataTransferObject
                {
                    Id = transaction.Id,
                    Direction = DomainEnumMapper.ToApplication(transaction.Direction),
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Description = transaction.Description,
                    MerchantName = transaction.MerchantName,
                    CounterpartyName = transaction.CounterpartyName,
                    Status = DomainEnumMapper.ToApplication(transaction.Status),
                    CreatedAt = transaction.CreatedAt,
                })
                .ToList(),
            UnreadNotificationCount = notifCountResult.IsError ? default : notifCountResult.Value,
        };
    }
}
