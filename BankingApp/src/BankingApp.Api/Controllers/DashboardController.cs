namespace BankingApp.Api.Controllers;

using Application.DTOs.Dashboard;
using Application.Repositories.Interfaces;
using Application.Services.Dashboard;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Controller responsible for handling dashboard-related operations.
///     All endpoints are accessible under the /api/dashboard route.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IDashboardService _dashboardService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DashboardController" /> class.
    /// </summary>
    /// <param name="dashboardService">The dashboard service used to handle business logic.</param>
    /// <param name="dashboardRepository">The dashboard repository used by the raw proxy endpoints.</param>
    /// <returns>The result of the operation.</returns>
    public DashboardController(IDashboardService dashboardService, IDashboardRepository dashboardRepository)
    {
        _dashboardService = dashboardService;
        _dashboardRepository = dashboardRepository;
    }

    /// <summary>
    ///     Retrieves the dashboard data for the currently authenticated user.
    ///     The user ID is extracted from the HTTP context, set by the authentication middleware.
    /// </summary>
    /// <returns>
    ///     200 OK with a <see cref="DashboardDto" /> on success,
    ///     or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet]
    public IActionResult GetDashboard()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_dashboardService.GetDashboardData(userId), Ok);
    }

    /// <summary>
    ///     Returns raw accounts for the supplied user identifier.
    /// </summary>
    [HttpGet("raw/accounts/{userId:int}")]
    public IActionResult GetAccountsByUserIdRaw(int userId)
    {
        return ToActionResult(_dashboardRepository.GetAccountsByUser(userId), Ok);
    }

    /// <summary>
    ///     Returns raw cards for the supplied user identifier.
    /// </summary>
    [HttpGet("raw/cards/{userId:int}")]
    public IActionResult GetCardsByUserIdRaw(int userId)
    {
        return ToActionResult(_dashboardRepository.GetCardsByUser(userId), Ok);
    }

    /// <summary>
    ///     Returns recent raw transactions for the supplied account identifier.
    /// </summary>
    [HttpGet("raw/transactions/{accountId:int}")]
    public IActionResult GetRecentTransactionsRaw(int accountId, [FromQuery] int limit = IDashboardRepository.DefaultRecentTransactionLimit)
    {
        return ToActionResult(_dashboardRepository.GetRecentTransactions(accountId, limit), Ok);
    }

    /// <summary>
    ///     Returns the unread notification count for the supplied user identifier.
    /// </summary>
    [HttpGet("raw/notifications/{userId:int}/unread-count")]
    public IActionResult GetUnreadNotificationCountRaw(int userId)
    {
        return ToActionResult(_dashboardRepository.GetUnreadNotificationCount(userId), count => Ok(count));
    }

    /// <summary>
    ///     Returns raw transfers for the supplied user identifier.
    /// </summary>
    [HttpGet("raw/transfers/{userId:int}")]
    public IActionResult GetTransfersByUserIdRaw(int userId)
    {
        return ToActionResult(_dashboardRepository.GetTransfersByUserId(userId), Ok);
    }

    /// <summary>
    ///     Debits the specified account by the supplied amount.
    /// </summary>
    [HttpPost("raw/accounts/{accountId:int}/debit")]
    public IActionResult DebitAccountRaw(int accountId, [FromBody] DebitAccountRequest request)
    {
        return ToActionResult(_dashboardRepository.DebitAccount(accountId, request.Amount));
    }

    /// <summary>
    ///     Persists a raw transaction entity.
    /// </summary>
    [HttpPost("raw/transactions")]
    public IActionResult AddTransactionRaw([FromBody] Transaction transaction)
    {
        return ToActionResult(_dashboardRepository.AddTransaction(transaction), Ok);
    }

    /// <summary>
    ///     Persists a raw transfer entity.
    /// </summary>
    [HttpPost("raw/transfers")]
    public IActionResult AddTransferRaw([FromBody] Transfer transfer)
    {
        return ToActionResult(_dashboardRepository.AddTransfer(transfer), Ok);
    }

    /// <summary>
    ///     Request body for raw account debit operations.
    /// </summary>
    public sealed class DebitAccountRequest
    {
        /// <summary>
        ///     Gets or sets the amount to debit.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
