namespace BankingApp.Web.Controllers;

using Contracts.Features.Beneficiaries.Dtos;
using Contracts.Features.Beneficiaries.Services;
using ErrorOr;
using ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class BeneficiariesController(IBeneficiaryService beneficiaryService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ErrorOr<List<BeneficiaryDto>> result = await beneficiaryService.GetAllAsync(ct);
        if (result.IsError)
        {
            TempData["Error"] = "Could not load beneficiaries.";
            return View(new BeneficiaryListViewModel());
        }

        IReadOnlyList<BeneficiaryListViewModel.BeneficiaryRow> rows = result.Value
            .ConvertAll(dto => new BeneficiaryListViewModel.BeneficiaryRow
            {
                Id = dto.Id,
                Name = dto.Name ?? string.Empty,
                Iban = dto.Iban ?? string.Empty,
                BankName = dto.BankName,
                TransferCount = dto.TransferCount,
                TotalAmountSent = dto.TotalAmountSent,
                LastTransferDate = dto.LastTransferDate
            });

        return View(new BeneficiaryListViewModel { Beneficiaries = rows });
    }

    public IActionResult Create() => View(new CreateBeneficiaryViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBeneficiaryViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        CreateBeneficiaryRequest request = new() { Name = model.Name, Iban = model.Iban, BankName = model.BankName };
        ErrorOr<Success> createResult = await beneficiaryService.CreateAsync(request, ct);
        if (createResult.IsError)
        {
            ModelState.AddModelError(string.Empty, "Could not save beneficiary. Check that the IBAN is valid and not already saved.");
            return View(model);
        }

        TempData["Success"] = $"{model.Name} was added to your beneficiaries.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ErrorOr<BeneficiaryDto> result = await beneficiaryService.GetByIdAsync(id, ct);
        if (result.IsError)
        {
            return NotFound();
        }

        BeneficiaryDto dto = result.Value;
        return View(new EditBeneficiaryViewModel
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            Iban = dto.Iban ?? string.Empty,
            BankName = dto.BankName
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditBeneficiaryViewModel model, CancellationToken ct)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        UpdateBeneficiaryRequest request = new()
        {
            Id = id,
            Name = model.Name,
            Iban = model.Iban,
            BankName = model.BankName
        };
        ErrorOr<Success> updateResult = await beneficiaryService.UpdateAsync(id, request, ct);
        if (updateResult.IsError)
        {
            ModelState.AddModelError(string.Empty, "Could not update beneficiary. The IBAN may be invalid.");
            return View(model);
        }

        TempData["Success"] = $"{model.Name} was updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        ErrorOr<Success> result = await beneficiaryService.DeleteAsync(id, ct);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? "Could not remove beneficiary."
            : "Beneficiary removed.";

        return RedirectToAction(nameof(Index));
    }
}
