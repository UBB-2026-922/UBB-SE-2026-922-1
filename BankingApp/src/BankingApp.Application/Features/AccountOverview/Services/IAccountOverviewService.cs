namespace BankingApp.Application.Features.AccountOverview.Services;

using ErrorOr;

using BankingApp.Application.Features.AccountOverview.Dtos;

/// <summary>
///     Defines operations for aggregating dashboard data for a user.
/// </summary>
public interface IAccountOverviewService
{
    /// <summary>
    ///     Retrieves the full dashboard data for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>
    ///     A <see cref="AccountOverviewDto" /> containing the user summary, cards, recent transactions,
    ///     and unread notification count on success,
    ///     or a not-found error if the user does not exist.
    ///     Individual sub-queries (cards, transactions, notification count) that fail are logged
    ///     and degraded to empty lists or zero rather than propagated as errors.
    /// </returns>
    public ErrorOr<AccountOverviewDto> GetAccountOverview(int userId);
}
