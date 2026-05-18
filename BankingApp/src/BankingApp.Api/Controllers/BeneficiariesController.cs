namespace BankingApp.Api.Controllers;

using Application.Features.Beneficiaries.Commands;
using Application.Features.Beneficiaries.Dtos;
using Application.Features.Beneficiaries.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Manages the authenticated user's saved beneficiaries.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BeneficiariesController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBeneficiaries(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetBeneficiariesQuery(userId), cancellationToken), Ok);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBeneficiary(
        [FromBody] CreateBeneficiaryRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var command = new CreateBeneficiaryCommand(userId, request.Name, request.Iban, request.BankName);
        return ToActionResult(await Sender.Send(command, cancellationToken), Ok);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBeneficiary(
        int id,
        [FromBody] UpdateBeneficiaryRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var command = new UpdateBeneficiaryCommand(userId, id, request.Name, request.Iban, request.BankName);
        return ToActionResult(await Sender.Send(command, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBeneficiary(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new DeleteBeneficiaryCommand(userId, id), cancellationToken));
    }
}
