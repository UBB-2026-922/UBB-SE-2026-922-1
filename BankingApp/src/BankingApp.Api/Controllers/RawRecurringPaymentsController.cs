#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/raw/recurring-payments")]
public class RawRecurringPaymentsController : ApiControllerBase
{
    private readonly IRecurringPaymentRepository _recurringPaymentRepository;

    public RawRecurringPaymentsController(IRecurringPaymentRepository recurringPaymentRepository)
    {
        _recurringPaymentRepository = recurringPaymentRepository;
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
        => ToActionResult(_recurringPaymentRepository.GetById(id), Ok);

    [HttpGet("user/{userId:int}")]
    public IActionResult GetByUserId(int userId)
        => ToActionResult(_recurringPaymentRepository.GetByUserId(userId), Ok);

    [HttpGet("due")]
    public IActionResult GetDuePayments([FromQuery] DateTime asOf)
        => ToActionResult(_recurringPaymentRepository.GetDuePayments(asOf), Ok);

    [HttpPost]
    public IActionResult Create([FromBody] RecurringPayment payment)
        => ToActionResult(_recurringPaymentRepository.Create(payment), Ok);

    [HttpPut]
    public IActionResult Update([FromBody] RecurringPayment payment)
        => ToActionResult(_recurringPaymentRepository.Update(payment));
}
#pragma warning restore CS1591
