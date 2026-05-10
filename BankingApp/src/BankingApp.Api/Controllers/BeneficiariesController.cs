#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.DTOs.Beneficiaries;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BeneficiariesController(IBeneficiaryRepository beneficiaryRepository) : ApiController
{
    [HttpGet]
    public IActionResult GetBeneficiaries()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            beneficiaryRepository.FindByUserId(userId),
            beneficiaries => Ok(beneficiaries.ConvertAll(MapToDto)));
    }

    [HttpGet("{beneficiaryId:int}")]
    public IActionResult GetBeneficiaryById(int beneficiaryId)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            beneficiaryRepository.FindById(beneficiaryId, userId),
            beneficiary => Ok(MapToDto(beneficiary)));
    }

    [HttpPost]
    public IActionResult CreateBeneficiary([FromBody] CreateBeneficiaryRequest request)
    {
        int userId = GetAuthenticatedUserId();
        var beneficiary = new Beneficiary
        {
            User = new User { Id = userId },
            Name = request.Name.Trim(),
            Iban = request.Iban.Trim().ToUpperInvariant(),
            BankName = string.IsNullOrWhiteSpace(request.BankName) ? null : request.BankName.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
        return ToActionResult(beneficiaryRepository.Create(beneficiary), b => Ok(MapToDto(b)));
    }

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
        return ToActionResult(beneficiaryRepository.Update(beneficiary));
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteBeneficiary(int id)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(beneficiaryRepository.Delete(id, userId));
    }

    [HttpGet("raw/{userId:int}")]
    public IActionResult GetBeneficiariesRaw(int userId)
        => ToActionResult(beneficiaryRepository.FindByUserId(userId), Ok);

    [HttpGet("raw/{userId:int}/{beneficiaryId:int}")]
    public IActionResult GetBeneficiaryRaw(int userId, int beneficiaryId)
        => ToActionResult(beneficiaryRepository.FindById(beneficiaryId, userId), Ok);

    [HttpGet("raw/{userId:int}/exists")]
    public IActionResult BeneficiaryExistsRaw(int userId, [FromQuery] string iban)
        => ToActionResult(beneficiaryRepository.ExistsByUserIdAndIban(userId, iban), exists => Ok(exists));

    [HttpPost("raw")]
    public IActionResult CreateBeneficiaryRaw([FromBody] Beneficiary beneficiary)
        => ToActionResult(beneficiaryRepository.Create(beneficiary), Ok);

    [HttpPut("raw")]
    public IActionResult UpdateBeneficiaryRaw([FromBody] Beneficiary beneficiary)
        => ToActionResult(beneficiaryRepository.Update(beneficiary));

    [HttpDelete("raw/{userId:int}/{beneficiaryId:int}")]
    public IActionResult DeleteBeneficiaryRaw(int userId, int beneficiaryId)
        => ToActionResult(beneficiaryRepository.Delete(beneficiaryId, userId));

    private static BeneficiaryDto MapToDto(Beneficiary beneficiary) => new()
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
#pragma warning restore CS1591
