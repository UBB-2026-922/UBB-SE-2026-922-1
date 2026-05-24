namespace BankingApp.Web.ViewModels;

using BankingApp.Contracts.Features.AccountOverview.Dtos;

public sealed class DashboardViewModel
{
    public UserSummaryDto UserSummary { get; init; } = new();
    public IList<CardDto> Cards { get; init; } = [];
    public IList<TransactionDto> RecentTransactions { get; init; } = [];
    public int UnreadNotificationCount { get; init; }
}