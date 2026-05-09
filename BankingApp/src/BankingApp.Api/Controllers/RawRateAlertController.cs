#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/raw/rate-alerts")]
public class RawRateAlertController : ApiControllerBase
{
    private readonly IRateAlertRepository _rateAlertRepository;

    public RawRateAlertController(IRateAlertRepository rateAlertRepository)
    {
        _rateAlertRepository = rateAlertRepository;
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
        => ToActionResult(_rateAlertRepository.GetById(id), Ok);

    [HttpGet("user/{userId:int}")]
    public IActionResult GetByUserId(int userId)
        => ToActionResult(_rateAlertRepository.GetByUserId(userId), Ok);

    [HttpGet("untriggered")]
    public IActionResult GetUntriggeredAlerts()
        => ToActionResult(_rateAlertRepository.GetUntriggeredAlerts(), Ok);

    [HttpPost]
    public IActionResult Create([FromBody] RateAlert alert)
        => ToActionResult(_rateAlertRepository.Create(alert), Ok);

    [HttpPut("{alertId:int}/mark-triggered")]
    public IActionResult MarkTriggered(int alertId)
        => ToActionResult(_rateAlertRepository.MarkTriggered(alertId), Ok);

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
        => ToActionResult(_rateAlertRepository.Delete(id));
}
#pragma warning restore CS1591
