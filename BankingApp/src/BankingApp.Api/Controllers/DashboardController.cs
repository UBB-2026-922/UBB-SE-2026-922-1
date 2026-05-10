namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Controller responsible for handling dashboard-related operations.
///     All endpoints are accessible under the /api/dashboard route.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DashboardController(IDashboardRepository dashboardRepository) : ApiController
{
    /// <summary>
    ///     Returns accounts for the supplied user identifier.
    /// </summary>
    [HttpGet("accounts/{userId:int}")]
    public IActionResult GetAccountsByUserIdRaw(int userId)
    {
        return ToActionResult(dashboardRepository.GetAccountsByUser(userId), Ok);
    }

    /// <summary>
    ///     Returns cards for the supplied user identifier.
    /// </summary>
    [HttpGet("cards/{userId:int}")]
    public IActionResult GetCardsByUserIdRaw(int userId)
    {
        return ToActionResult(dashboardRepository.GetCardsByUser(userId), Ok);
    }

    /// <summary>
    ///     Returns recent transactions for the supplied account identifier.
    /// </summary>
    [HttpGet("transactions/{accountId:int}")]
    public IActionResult GetRecentTransactionsRaw(int accountId, [FromQuery] int limit = IDashboardRepository.DefaultRecentTransactionLimit)
    {
        return ToActionResult(dashboardRepository.GetRecentTransactions(accountId, limit), Ok);
    }

    /// <summary>
    ///     Returns the unread notification count for the supplied user identifier.
    /// </summary>
    [HttpGet("notifications/{userId:int}/unread-count")]
    public IActionResult GetUnreadNotificationCountRaw(int userId)
    {
        return ToActionResult(dashboardRepository.GetUnreadNotificationCount(userId), count => Ok(count));
    }

    /// <summary>
    ///     Debits the specified account by the supplied amount.
    /// </summary>
    [HttpPost("accounts/{accountId:int}/debit")]
    public IActionResult DebitAccountRaw(int accountId, [FromBody] DebitAccountRequest request)
    {
        return ToActionResult(dashboardRepository.DebitAccount(accountId, request.Amount));
    }

    /// <summary>
    ///     Persists a transaction entity.
    /// </summary>
    [HttpPost("transactions")]
    public IActionResult AddTransactionRaw([FromBody] Transaction transaction)
    {
        return ToActionResult(dashboardRepository.AddTransaction(transaction), Ok);
    }

    /// <summary>
    ///     Request body for account debit operations.
    /// </summary>
    public sealed class DebitAccountRequest
    {
        /// <summary>
        ///     Gets or sets the amount to debit.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
