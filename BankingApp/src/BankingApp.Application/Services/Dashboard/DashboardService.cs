namespace BankingApp.Application.Services.Dashboard;

using Logging;
using Repositories.Interfaces;
using Domain.Entities;
using DTOs.Dashboard;
using ErrorOr;
using Microsoft.Extensions.Logging;

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
    public ErrorOr<DashboardDto> GetDashboardData(int userId)
    {
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.DashboardUserNotFound(userId);
            return userResult.FirstError;
        }

        ErrorOr<List<Card>> cardsResult = _dashboardRepository.GetCardsByUser(userId);
        ErrorOr<int> notifCountResult = _dashboardRepository.GetUnreadNotificationCount(userId);
        if (cardsResult.IsError)
        {
            _logger.DashboardFetchCardsFailed(userId, cardsResult.FirstError.Description);
        }

        if (notifCountResult.IsError)
        {
            _logger.DashboardFetchNotificationCountFailed(userId, notifCountResult.FirstError.Description);
        }

        var allTransactions = new List<Transaction>();
        ErrorOr<List<Account>> accountsResult = _dashboardRepository.GetAccountsByUser(userId);
        var accountsById = new Dictionary<int, Account>();
        if (accountsResult.IsError)
        {
            _logger.DashboardFetchAccountsFailed(userId, accountsResult.FirstError.Description);
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
                    _logger.DashboardFetchTransactionsFailed(account.Id, transactionsResult.FirstError.Description);
                    continue;
                }

                allTransactions.AddRange(transactionsResult.Value);
            }

            allTransactions = allTransactions
                .OrderByDescending(transaction => transaction.CreatedAt)
                .Take(DefaultRecentTransactionLimit)
                .ToList();
        }

        return new DashboardDto
        {
            CurrentUser = new UserSummaryDto
            {
                FullName = userResult.Value.FullName,
                Email = userResult.Value.Email,
                PhoneNumber = userResult.Value.PhoneNumber,
                Is2FaEnabled = userResult.Value.Is2FaEnabled
            },
            Cards = cardsResult.IsError
                ? new List<CardDto>()
                : cardsResult.Value
                    .Select(card => new CardDto
                    {
                        CardNumber = card.GetMaskedNumber(),
                        CardholderName = card.CardholderName,
                        CardType = card.CardType,
                        CardBrand = card.CardBrand,
                        ExpiryDate = card.ExpiryDate,
                        Status = card.Status,
                        IsContactlessEnabled = card.IsContactlessEnabled,
                        IsOnlineEnabled = card.IsOnlineEnabled,
                        AccountName = accountsById.TryGetValue(card.AccountId, out Account? account)
                            ? account.AccountName
                            : null,
                        AccountBalance = account?.Balance
                    })
                    .ToList(),
            RecentTransactions = allTransactions
                .Select(transaction => new TransactionDto
                {
                    Id = transaction.Id,
                    Direction = transaction.Direction,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Description = transaction.Description,
                    MerchantName = transaction.MerchantName,
                    CounterpartyName = transaction.CounterpartyName,
                    Status = transaction.Status,
                    CreatedAt = transaction.CreatedAt
                })
                .ToList(),
            UnreadNotificationCount = notifCountResult.IsError ? 0 : notifCountResult.Value
        };
    }
}
