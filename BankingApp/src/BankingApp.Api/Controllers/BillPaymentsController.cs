#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/bill-payments")]
public class BillPaymentsController(IBillPaymentRepository billPaymentRepository) : ApiController
{
    [HttpGet("billers")]
    public async Task<IActionResult> GetBillersAsync()
        => Ok((await billPaymentRepository.GetBillersAsync()).ToList());

    [HttpGet("billers/{billerId:int}")]
    public async Task<IActionResult> GetBillerByIdAsync(int billerId)
    {
        Biller? biller = await billPaymentRepository.GetBillerByIdAsync(billerId);
        return biller is null ? NotFound() : Ok(biller);
    }

    [HttpPost("payments")]
    public async Task<IActionResult> AddPaymentAsync([FromBody] BillPayment payment)
    {
        await billPaymentRepository.AddPaymentAsync(payment);
        return Ok(payment);
    }

    [HttpGet("payments/user/{userId:int}")]
    public async Task<IActionResult> GetUserPaymentHistoryAsync(int userId)
        => Ok((await billPaymentRepository.GetUserPaymentHistoryAsync(userId)).ToList());

    [HttpGet("saved-billers/{userId:int}")]
    public async Task<IActionResult> GetSavedBillersAsync(int userId)
        => Ok((await billPaymentRepository.GetSavedBillersAsync(userId)).ToList());

    [HttpPost("saved-billers")]
    public async Task<IActionResult> AddSavedBillerAsync([FromBody] SavedBiller savedBiller)
    {
        await billPaymentRepository.AddSavedBillerAsync(savedBiller);
        return Ok(savedBiller);
    }

    [HttpGet("accounts/{accountId:int}")]
    public async Task<IActionResult> GetAccountByIdAsync(int accountId)
    {
        Account? account = await billPaymentRepository.GetAccountByIdAsync(accountId);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpGet("accounts/user/{userId:int}")]
    public async Task<IActionResult> GetAccountsByUserIdAsync(int userId)
        => Ok((await billPaymentRepository.GetAccountsByUserIdAsync(userId)).ToList());

    [HttpPut("accounts")]
    public async Task<IActionResult> UpdateAccountAsync([FromBody] Account account)
    {
        await billPaymentRepository.UpdateAccountAsync(account);
        return NoContent();
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> AddTransactionAsync([FromBody] Transaction transaction)
    {
        await billPaymentRepository.AddTransactionAsync(transaction);
        return Ok(transaction);
    }
}
#pragma warning restore CS1591
