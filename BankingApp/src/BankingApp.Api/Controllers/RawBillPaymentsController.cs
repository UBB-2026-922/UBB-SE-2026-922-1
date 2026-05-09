#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/raw/bill-payments")]
public class RawBillPaymentsController : ApiControllerBase
{
    private readonly IBillPaymentRepository _billPaymentRepository;

    public RawBillPaymentsController(IBillPaymentRepository billPaymentRepository)
    {
        _billPaymentRepository = billPaymentRepository;
    }

    [HttpGet("billers")]
    public async Task<IActionResult> GetBillersAsync()
        => Ok((await _billPaymentRepository.GetBillersAsync()).ToList());

    [HttpGet("billers/{billerId:int}")]
    public async Task<IActionResult> GetBillerByIdAsync(int billerId)
    {
        Biller? biller = await _billPaymentRepository.GetBillerByIdAsync(billerId);
        return biller is null ? NotFound() : Ok(biller);
    }

    [HttpPost("payments")]
    public async Task<IActionResult> AddPaymentAsync([FromBody] BillPayment payment)
    {
        await _billPaymentRepository.AddPaymentAsync(payment);
        return Ok(payment);
    }

    [HttpGet("payments/user/{userId:int}")]
    public async Task<IActionResult> GetUserPaymentHistoryAsync(int userId)
        => Ok((await _billPaymentRepository.GetUserPaymentHistoryAsync(userId)).ToList());

    [HttpGet("saved-billers/{userId:int}")]
    public async Task<IActionResult> GetSavedBillersAsync(int userId)
        => Ok((await _billPaymentRepository.GetSavedBillersAsync(userId)).ToList());

    [HttpPost("saved-billers")]
    public async Task<IActionResult> AddSavedBillerAsync([FromBody] SavedBiller savedBiller)
    {
        await _billPaymentRepository.AddSavedBillerAsync(savedBiller);
        return Ok(savedBiller);
    }

    [HttpGet("accounts/{accountId:int}")]
    public async Task<IActionResult> GetAccountByIdAsync(int accountId)
    {
        Account? account = await _billPaymentRepository.GetAccountByIdAsync(accountId);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpGet("accounts/user/{userId:int}")]
    public async Task<IActionResult> GetAccountsByUserIdAsync(int userId)
        => Ok((await _billPaymentRepository.GetAccountsByUserIdAsync(userId)).ToList());

    [HttpPut("accounts")]
    public async Task<IActionResult> UpdateAccountAsync([FromBody] Account account)
    {
        await _billPaymentRepository.UpdateAccountAsync(account);
        return NoContent();
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> AddTransactionAsync([FromBody] Transaction transaction)
    {
        await _billPaymentRepository.AddTransactionAsync(transaction);
        return Ok(transaction);
    }
}
#pragma warning restore CS1591
