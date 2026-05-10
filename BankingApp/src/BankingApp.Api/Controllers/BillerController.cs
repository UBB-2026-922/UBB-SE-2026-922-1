#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/raw/billers")]
public class BillerController : ApiController
{
    private readonly IBillerRepository _billerRepository;

    public BillerController(IBillerRepository billerRepository)
    {
        _billerRepository = billerRepository;
    }

    [HttpGet]
    public IActionResult GetAllBillers([FromQuery] bool activeOnly = true)
        => ToActionResult(_billerRepository.GetAllBillers(activeOnly), Ok);

    [HttpGet("search")]
    public IActionResult SearchBillers([FromQuery] string searchTerm, [FromQuery] string? category = null, [FromQuery] bool activeOnly = true)
        => ToActionResult(_billerRepository.SearchBillers(searchTerm, category, activeOnly), Ok);

    [HttpGet("{billerId:int}")]
    public IActionResult GetBillerById(int billerId)
        => ToActionResult(_billerRepository.GetBillerById(billerId), Ok);

    [HttpGet("saved/{userId:int}")]
    public IActionResult GetSavedBillers(int userId)
        => ToActionResult(_billerRepository.GetSavedBillers(userId), Ok);

    [HttpPost("saved")]
    public IActionResult SaveBiller([FromBody] SavedBiller savedBiller)
        => ToActionResult(_billerRepository.SaveBiller(savedBiller), Ok);

    [HttpDelete("saved/{savedBillerId:int}")]
    public IActionResult DeleteSavedBiller(int savedBillerId)
        => ToActionResult(_billerRepository.DeleteSavedBiller(savedBillerId));
}
#pragma warning restore CS1591
