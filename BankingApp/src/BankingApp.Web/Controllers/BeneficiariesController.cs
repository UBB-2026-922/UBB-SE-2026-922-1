namespace BankingApp.Web.Controllers;

using BankingApp.Web.Services;
using BankingApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class BeneficiariesController : Controller
{
    private readonly IBeneficiaryService _beneficiaryService;

    public BeneficiariesController(IBeneficiaryService beneficiaryService)
    {
        _beneficiaryService = beneficiaryService;
    }

    private string Token => User.FindFirst("token")!.Value;

    public async Task<IActionResult> Index()
    {
        IReadOnlyList<BeneficiaryListViewModel.BeneficiaryRow> rows =
            await _beneficiaryService.GetAllAsync(Token);

        return View(new BeneficiaryListViewModel { Beneficiaries = rows });
    }

    public IActionResult Create() => View(new CreateBeneficiaryViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBeneficiaryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        bool success = await _beneficiaryService.CreateAsync(model, Token);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Could not save beneficiary. Check that the IBAN is valid and not already saved.");
            return View(model);
        }

        TempData["Success"] = $"{model.Name} was added to your beneficiaries.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        EditBeneficiaryViewModel? model = await _beneficiaryService.GetByIdAsync(id, Token);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditBeneficiaryViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        bool success = await _beneficiaryService.UpdateAsync(model, Token);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Could not update beneficiary. The IBAN may be invalid.");
            return View(model);
        }

        TempData["Success"] = $"{model.Name} was updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        bool success = await _beneficiaryService.DeleteAsync(id, Token);

        TempData[success ? "Success" : "Error"] = success
            ? "Beneficiary removed."
            : "Could not remove beneficiary.";

        return RedirectToAction(nameof(Index));
    }
}
