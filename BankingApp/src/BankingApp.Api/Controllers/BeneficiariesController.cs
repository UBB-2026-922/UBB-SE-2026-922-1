namespace BankingApp.Api.Controllers;

using Application.DTOs.Beneficiary;
using Application.Services.Beneficiary;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Controller responsible for managing beneficiaries for the authenticated user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BeneficiariesController : ApiControllerBase
{
    private readonly IBeneficiaryService _beneficiaryService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BeneficiariesController" /> class.
    /// </summary>
    /// <param name="beneficiaryService">The beneficiary service.</param>
    public BeneficiariesController(IBeneficiaryService beneficiaryService)
    {
        _beneficiaryService = beneficiaryService;
    }

    /// <summary>
    ///     Gets all beneficiaries for the authenticated user.
    /// </summary>
    /// <returns>
    ///     200 OK with the user's beneficiaries, or an error otherwise.
    /// </returns>
    [HttpGet]
    public IActionResult GetBeneficiaries()
    {
        int userId = GetAuthenticatedUserId();

        return ToActionResult(
            _beneficiaryService.GetByUserId(userId),
            beneficiaries => Ok(beneficiaries.Select(MapToDto).ToList()));
    }

    /// <summary>
    ///     Gets a beneficiary by identifier.
    /// </summary>
    /// <param name="id">The beneficiary identifier.</param>
    /// <returns>
    ///     200 OK with the beneficiary when found, or an error otherwise.
    /// </returns>
    [HttpGet("{id:int}")]
    public IActionResult GetBeneficiaryById(int id)
    {
        int userId = GetAuthenticatedUserId();

        return ToActionResult(
            _beneficiaryService.GetById(id, userId),
            beneficiary => Ok(MapToDto(beneficiary)));
    }

    /// <summary>
    ///     Creates a new beneficiary for the authenticated user.
    /// </summary>
    /// <param name="request">The beneficiary creation request.</param>
    /// <returns>
    ///     200 OK with the created beneficiary, or an error otherwise.
    /// </returns>
    [HttpPost]
    public IActionResult CreateBeneficiary([FromBody] CreateBeneficiaryRequest request)
    {
        int userId = GetAuthenticatedUserId();

        return ToActionResult(
            _beneficiaryService.Create(userId, request.Name, request.Iban, request.BankName),
            beneficiary => Ok(MapToDto(beneficiary)));
    }

    /// <summary>
    ///     Updates an existing beneficiary.
    /// </summary>
    /// <param name="id">The beneficiary identifier from the route.</param>
    /// <param name="request">The beneficiary update request.</param>
    /// <returns>
    ///     204 No Content on success, or an error otherwise.
    /// </returns>
    [HttpPut("{id:int}")]
    public IActionResult UpdateBeneficiary(int id, [FromBody] UpdateBeneficiaryRequest request)
    {
        int userId = GetAuthenticatedUserId();

        var beneficiary = new Beneficiary
        {
            Id = id,
            UserId = userId,
            Name = request.Name,
            Iban = request.Iban,
            BankName = request.BankName,
        };

        return ToActionResult(_beneficiaryService.Update(beneficiary));
    }

    /// <summary>
    ///     Deletes a beneficiary by identifier.
    /// </summary>
    /// <param name="id">The beneficiary identifier.</param>
    /// <returns>
    ///     204 No Content on success, or an error otherwise.
    /// </returns>
    [HttpDelete("{id:int}")]
    public IActionResult DeleteBeneficiary(int id)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_beneficiaryService.Delete(id, userId));
    }

    private static BeneficiaryDataTransferObject MapToDto(Beneficiary beneficiary)
    {
        return new BeneficiaryDataTransferObject
        {
            Id = beneficiary.Id,
            Name = beneficiary.Name,
            Iban = beneficiary.Iban,
            BankName = beneficiary.BankName,
            LastTransferDate = beneficiary.LastTransferDate,
            TotalAmountSent = beneficiary.TotalAmountSent,
            TransferCount = beneficiary.TransferCount,
            CreatedAt = beneficiary.CreatedAt,
        };
    }
}