#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/transfers")]
public class TransferController(ITransferRepository transferRepository) : ApiController
{
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
        => ToActionResult(transferRepository.GetById(id), Ok);

    [HttpGet("user/{userId:int}")]
    public IActionResult GetByUserId(int userId)
        => ToActionResult(transferRepository.GetByUserId(userId), Ok);

    [HttpPost]
    public IActionResult Create([FromBody] Transfer transfer)
        => ToActionResult(transferRepository.Create(transfer), result => CreatedAtAction(nameof(GetById), new { id = result.Id }, result));

    [HttpPut("{transferId:int}/status")]
    public IActionResult UpdateStatus(int transferId, [FromQuery] TransferStatus status)
        => ToActionResult(transferRepository.UpdateStatus(transferId, status), Ok);
}
