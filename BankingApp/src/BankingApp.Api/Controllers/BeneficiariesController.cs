namespace BankingApp.Api.Controllers;

using Application.DTOs.Beneficiaries;
using Application.Repositories.Interfaces;
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
    private readonly IBeneficiaryRepository _beneficiaryRepository;
    private readonly IBeneficiaryService _beneficiaryService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BeneficiariesController" /> class.
    /// </summary>
    /// <param name="beneficiaryService">The beneficiary service.</param>
    /// <param name="beneficiaryRepository">The beneficiary repository used by the raw proxy endpoints.</param>
    public BeneficiariesController(IBeneficiaryService beneficiaryService, IBeneficiaryRepository beneficiaryRepository)
    {
        _beneficiaryService = beneficiaryService;
        _beneficiaryRepository = beneficiaryRepository;
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
            User = new User { Id = userId },
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

    /// <summary>
    ///     Returns raw beneficiaries for the supplied user identifier.
    /// </summary>
    [HttpGet("raw/{userId:int}")]
    public IActionResult GetBeneficiariesRaw(int userId)
    {
        return ToActionResult(_beneficiaryRepository.FindByUserId(userId), Ok);
    }

    /// <summary>
    ///     Returns a raw beneficiary for the supplied user and beneficiary identifiers.
    /// </summary>
    [HttpGet("raw/{userId:int}/{beneficiaryId:int}")]
    public IActionResult GetBeneficiaryRaw(int userId, int beneficiaryId)
    {
        return ToActionResult(_beneficiaryRepository.FindById(beneficiaryId, userId), Ok);
    }

    /// <summary>
    ///     Returns whether a beneficiary with the supplied IBAN already exists for the user.
    /// </summary>
    [HttpGet("raw/{userId:int}/exists")]
    public IActionResult BeneficiaryExistsRaw(int userId, [FromQuery] string iban)
    {
        return ToActionResult(_beneficiaryRepository.ExistsByUserIdAndIban(userId, iban), exists => Ok(exists));
    }

    /// <summary>
    ///     Persists a raw beneficiary entity.
    /// </summary>
    [HttpPost("raw")]
    public IActionResult CreateBeneficiaryRaw([FromBody] Beneficiary beneficiary)
    {
        return ToActionResult(_beneficiaryRepository.Create(beneficiary), Ok);
    }

    /// <summary>
    ///     Updates a raw beneficiary entity.
    /// </summary>
    [HttpPut("raw")]
    public IActionResult UpdateBeneficiaryRaw([FromBody] Beneficiary beneficiary)
    {
        return ToActionResult(_beneficiaryRepository.Update(beneficiary));
    }

    /// <summary>
    ///     Deletes a raw beneficiary by user and beneficiary identifiers.
    /// </summary>
    [HttpDelete("raw/{userId:int}/{beneficiaryId:int}")]
    public IActionResult DeleteBeneficiaryRaw(int userId, int beneficiaryId)
    {
        return ToActionResult(_beneficiaryRepository.Delete(beneficiaryId, userId));
    }

    private static BeneficiaryDto MapToDto(Beneficiary beneficiary)
    {
        return new BeneficiaryDto
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
