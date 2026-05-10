#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/billers")]
public class BillerController(IBillerRepository billerRepository) : ApiController
{
    [HttpGet]
    public IActionResult GetAllBillers([FromQuery] bool activeOnly = true)
        => ToActionResult(billerRepository.GetAllBillers(activeOnly), Ok);

    [HttpGet("search")]
    public IActionResult SearchBillers([FromQuery] string searchTerm, [FromQuery] string? category = null, [FromQuery] bool activeOnly = true)
        => ToActionResult(billerRepository.SearchBillers(searchTerm, category, activeOnly), Ok);

    [HttpGet("{billerId:int}")]
    public IActionResult GetBillerById(int billerId)
        => ToActionResult(billerRepository.GetBillerById(billerId), Ok);

    [HttpGet("saved/{userId:int}")]
    public IActionResult GetSavedBillers(int userId)
        => ToActionResult(billerRepository.GetSavedBillers(userId), Ok);

    [HttpPost("saved")]
    public IActionResult SaveBiller([FromBody] SavedBiller savedBiller)
        => ToActionResult(billerRepository.SaveBiller(savedBiller), Ok);

    [HttpDelete("saved/{savedBillerId:int}")]
    public IActionResult DeleteSavedBiller(int savedBillerId)
        => ToActionResult(billerRepository.DeleteSavedBiller(savedBillerId));
}
#pragma warning restore CS1591
