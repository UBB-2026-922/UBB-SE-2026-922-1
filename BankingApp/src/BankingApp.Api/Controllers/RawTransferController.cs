#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/raw/transfers")]
public class RawTransferController : ApiControllerBase
{
    private readonly ITransferRepository _transferRepository;

    public RawTransferController(ITransferRepository transferRepository)
    {
        _transferRepository = transferRepository;
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
        => ToActionResult(_transferRepository.GetById(id), Ok);

    [HttpGet("user/{userId:int}")]
    public IActionResult GetByUserId(int userId)
        => ToActionResult(_transferRepository.GetByUserId(userId), Ok);

    [HttpPost]
    public IActionResult Create([FromBody] Transfer transfer)
        => ToActionResult(_transferRepository.Create(transfer), result => CreatedAtAction(nameof(GetById), new { id = result.Id }, result));

    [HttpPatch("{transferId:int}/status")]
    public IActionResult UpdateStatus(int transferId, [FromQuery] TransferStatus status)
        => ToActionResult(_transferRepository.UpdateStatus(transferId, status), Ok);
}
