#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/recurring-payments")]
public class RecurringPaymentsController(IRecurringPaymentRepository recurringPaymentRepository) : ApiController
{
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
        => ToActionResult(recurringPaymentRepository.GetById(id), Ok);

    [HttpGet("user/{userId:int}")]
    public IActionResult GetByUserId(int userId)
        => ToActionResult(recurringPaymentRepository.GetByUserId(userId), Ok);

    [HttpGet("due")]
    public IActionResult GetDuePayments([FromQuery] DateTime asOf)
        => ToActionResult(recurringPaymentRepository.GetDuePayments(asOf), Ok);

    [HttpPost]
    public IActionResult Create([FromBody] RecurringPayment payment)
        => ToActionResult(recurringPaymentRepository.Create(payment), Ok);

    [HttpPut]
    public IActionResult Update([FromBody] RecurringPayment payment)
        => ToActionResult(recurringPaymentRepository.Update(payment));
}
#pragma warning restore CS1591
