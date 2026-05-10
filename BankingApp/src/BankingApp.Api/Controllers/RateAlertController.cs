#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/rate-alerts")]
public class RateAlertController(IRateAlertRepository rateAlertRepository) : ApiController
{
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
        => ToActionResult(rateAlertRepository.GetById(id), Ok);

    [HttpGet("user/{userId:int}")]
    public IActionResult GetByUserId(int userId)
        => ToActionResult(rateAlertRepository.GetByUserId(userId), Ok);

    [HttpGet("untriggered")]
    public IActionResult GetUntriggeredAlerts()
        => ToActionResult(rateAlertRepository.GetUntriggeredAlerts(), Ok);

    [HttpPost]
    public IActionResult Create([FromBody] RateAlert alert)
        => ToActionResult(rateAlertRepository.Create(alert), Ok);

    [HttpPut("{alertId:int}/mark-triggered")]
    public IActionResult MarkTriggered(int alertId)
        => ToActionResult(rateAlertRepository.MarkTriggered(alertId), Ok);

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
        => ToActionResult(rateAlertRepository.Delete(id));
}
#pragma warning restore CS1591
