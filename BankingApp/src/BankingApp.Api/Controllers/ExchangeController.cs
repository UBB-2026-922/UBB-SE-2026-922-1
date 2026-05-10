#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/exchanges")]
public class ExchangeController(IExchangeRepository exchangeRepository) : ApiController
{
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
        => ToActionResult(exchangeRepository.GetById(id), Ok);

    [HttpGet("user/{userId:int}")]
    public IActionResult GetByUserId(int userId)
        => ToActionResult(exchangeRepository.GetByUserId(userId), Ok);

    [HttpPost]
    public IActionResult Create([FromBody] ExchangeTransaction exchange)
        => ToActionResult(exchangeRepository.Create(exchange), Ok);

    [HttpPut("{exchangeId:int}/status")]
    public IActionResult UpdateStatus(int exchangeId, [FromBody] UpdateStatusRequest request)
        => ToActionResult(exchangeRepository.UpdateStatus(exchangeId, request.Status), Ok);

    public sealed class UpdateStatusRequest
    {
        public ExchangeTransactionStatus Status { get; set; }
    }
}
#pragma warning restore CS1591
