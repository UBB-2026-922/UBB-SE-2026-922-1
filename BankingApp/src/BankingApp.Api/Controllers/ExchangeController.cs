#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/raw/exchanges")]
public class ExchangeController : ApiController
{
    private readonly IExchangeRepository _exchangeRepository;

    public ExchangeController(IExchangeRepository exchangeRepository)
    {
        _exchangeRepository = exchangeRepository;
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
        => ToActionResult(_exchangeRepository.GetById(id), Ok);

    [HttpGet("user/{userId:int}")]
    public IActionResult GetByUserId(int userId)
        => ToActionResult(_exchangeRepository.GetByUserId(userId), Ok);

    [HttpPost]
    public IActionResult Create([FromBody] ExchangeTransaction exchange)
        => ToActionResult(_exchangeRepository.Create(exchange), Ok);

    [HttpPut("{exchangeId:int}/status")]
    public IActionResult UpdateStatus(int exchangeId, [FromBody] UpdateStatusRequest request)
        => ToActionResult(_exchangeRepository.UpdateStatus(exchangeId, request.Status), Ok);

    public sealed class UpdateStatusRequest
    {
        public ExchangeTransactionStatus Status { get; set; }
    }
}
#pragma warning restore CS1591
