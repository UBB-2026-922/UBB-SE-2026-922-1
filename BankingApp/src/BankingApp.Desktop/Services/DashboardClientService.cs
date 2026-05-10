namespace BankingApp.Desktop.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Dashboard;
using Application.Repositories.Interfaces;
using Domain.Entities;
using ErrorOr;
using Utilities;

/// <summary>
///     Implements <see cref="IDashboardClientService" /> with desktop-side business logic and proxy repositories.
/// </summary>
internal sealed class DashboardClientService(
    ICurrentSession currentSession,
    IDashboardRepository dashboardRepository,
    IUserRepository userRepository)
    : IDashboardClientService
{
    private const int DefaultRecentTransactionLimit = 5;
    private readonly ICurrentSession _currentSession = currentSession ?? throw new ArgumentNullException(nameof(currentSession));

    private readonly IDashboardRepository _dashboardRepository =
        dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));

    private readonly IUserRepository _userRepository =
        userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    public Task<ErrorOr<DashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        int? userId = _currentSession.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<DashboardDto>>(
                Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<User> userResult = _userRepository.FindById(userId.Value);
        if (userResult.IsError)
        {
            return Task.FromResult<ErrorOr<DashboardDto>>(userResult.FirstError);
        }

        ErrorOr<List<Card>> cardsResult = _dashboardRepository.GetCardsByUser(userId.Value);
        ErrorOr<int> notifCountResult = _dashboardRepository.GetUnreadNotificationCount(userId.Value);
        var allTransactions = new List<Transaction>();
        var accountsById = new Dictionary<int, Account>();

        ErrorOr<List<Account>> accountsResult = _dashboardRepository.GetAccountsByUser(userId.Value);
        if (!accountsResult.IsError)
        {
            accountsById = accountsResult.Value.ToDictionary(account => account.Id);
            foreach (ErrorOr<List<Transaction>> transactionsResult in accountsResult.Value
                         .Select(account =>
                             _dashboardRepository.GetRecentTransactions(account.Id, DefaultRecentTransactionLimit))
                         .Where(transactionsResult => !transactionsResult.IsError))
            {
                allTransactions.AddRange(transactionsResult.Value);
            }

            allTransactions = allTransactions
                .OrderByDescending(transaction => transaction.CreatedAt)
                .Take(DefaultRecentTransactionLimit)
                .ToList();
        }

        var dashboard = new DashboardDto
        {
            CurrentUser = new UserSummaryDto
            {
                FullName = userResult.Value.FullName,
                Email = userResult.Value.Email,
                PhoneNumber = userResult.Value.PhoneNumber,
                Is2FaEnabled = userResult.Value.Is2FaEnabled,
            },
            Cards = cardsResult.IsError
                ? []
                : cardsResult.Value.Select(card => new CardDto
                {
                    CardNumber = card.GetMaskedNumber(),
                    CardholderName = card.CardholderName,
                    CardType = card.CardType,
                    CardBrand = card.CardBrand,
                    ExpiryDate = card.ExpiryDate,
                    Status = card.Status,
                    IsContactlessEnabled = card.IsContactlessEnabled,
                    IsOnlineEnabled = card.IsOnlineEnabled,
                    AccountName = card.Account?.AccountName
                                  ?? (card.Account is not null &&
                                      accountsById.TryGetValue(card.Account.Id, out Account? account)
                                      ? account.AccountName
                                      : null),
                    AccountBalance = card.Account?.Balance
                                     ?? (card.Account is not null &&
                                         accountsById.TryGetValue(card.Account.Id, out Account? linkedAccount)
                                         ? linkedAccount.Balance
                                         : null),
                }).ToList(),
            RecentTransactions = allTransactions.Select(transaction => new TransactionDto
            {
                Id = transaction.Id,
                Direction = transaction.Direction,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Description = transaction.Description,
                MerchantName = transaction.MerchantName,
                CounterpartyName = transaction.CounterpartyName,
                Status = transaction.Status,
                CreatedAt = transaction.CreatedAt,
            }).ToList(),
            UnreadNotificationCount = notifCountResult.IsError ? 0 : notifCountResult.Value,
        };

        return Task.FromResult<ErrorOr<DashboardDto>>(dashboard);
    }
}