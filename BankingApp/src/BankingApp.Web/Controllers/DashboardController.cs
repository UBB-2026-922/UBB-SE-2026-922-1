namespace BankingApp.Web.Controllers;

using BankingApp.Contracts.Features.AccountOverview.Dtos;
using BankingApp.Contracts.Features.AccountOverview.Services;
using BankingApp.Web.ViewModels;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class DashboardController(IAccountOverviewService accountOverviewService) : Controller
{
    private const int RecentTransactionLimit = 10;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ErrorOr<AccountOverviewDto> result = await accountOverviewService.GetDashboardAsync(cancellationToken);
        if (result.IsError)
        {
            TempData["Error"] = "Could not load dashboard data.";
            return View(new DashboardViewModel());
        }

        AccountOverviewDto dto = result.Value;
        var viewModel = new DashboardViewModel
        {
            UserSummary = dto.CurrentUser ?? new UserSummaryDto(),
            Cards = dto.Cards,
            RecentTransactions = dto.RecentTransactions
                .Take(RecentTransactionLimit)
                .ToList(),
            UnreadNotificationCount = dto.UnreadNotificationCount
        };

        return View(viewModel);
    }
}