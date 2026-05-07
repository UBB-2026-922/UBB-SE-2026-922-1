namespace BankingApp.Api.Controllers;

using Application.DTOs.Billers;
using Application.Services.Billers;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Controller for biller directory lookup and saved-biller CRUD.
///     All endpoints are accessible under the /api/billers route.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BillersController : ApiControllerBase
{
    private readonly IBillerService _billerService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillersController" /> class.
    /// </summary>
    /// <param name="billerService">The biller service.</param>
    public BillersController(IBillerService billerService)
    {
        _billerService = billerService;
    }

    /// <summary>
    ///     Returns the full active biller directory, or filters results by name and/or category.
    /// </summary>
    /// <param name="search">Optional name search term.</param>
    /// <param name="category">Optional category filter.</param>
    /// <returns>200 OK with the list of billers.</returns>
    [HttpGet]
    public IActionResult GetBillers([FromQuery] string? search, [FromQuery] string? category)
    {
        if (!string.IsNullOrWhiteSpace(search) || !string.IsNullOrWhiteSpace(category))
        {
            return ToActionResult(_billerService.SearchBillers(search ?? string.Empty, category), Ok);
        }

        return ToActionResult(_billerService.GetBillerDirectory(), Ok);
    }

    /// <summary>
    ///     Returns the billers saved by the currently authenticated user.
    /// </summary>
    /// <returns>200 OK with the list of saved billers.</returns>
    [HttpGet("saved")]
    public IActionResult GetSavedBillers()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_billerService.GetSavedBillers(userId), Ok);
    }

    /// <summary>
    ///     Saves a biller for the currently authenticated user.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>201 Created with the saved biller DTO.</returns>
    [HttpPost("saved")]
    public IActionResult SaveBiller([FromBody] SaveBillerRequest request)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            _billerService.SaveBiller(userId, request),
            data => CreatedAtAction(nameof(GetSavedBillers), data));
    }

    /// <summary>
    ///     Removes a saved biller for the currently authenticated user.
    /// </summary>
    /// <param name="id">The saved biller entry identifier.</param>
    /// <returns>204 No Content on success.</returns>
    [HttpDelete("saved/{id:int}")]
    public IActionResult RemoveSavedBiller(int id)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_billerService.RemoveSavedBiller(userId, id));
    }
}
