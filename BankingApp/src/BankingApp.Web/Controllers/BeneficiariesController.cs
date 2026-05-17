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

    // ── Index ────────────────────────────────────────────────────
    // GET /Beneficiaries
    public async Task<IActionResult> Index()
    {
        string? token = User.FindFirst("token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        IReadOnlyList<BeneficiaryListViewModel.BeneficiaryRow> rows =
            await _beneficiaryService.GetAllAsync(token);

        return View(new BeneficiaryListViewModel { Beneficiaries = rows });
    }

    // ── Create GET ───────────────────────────────────────────────
    // GET /Beneficiaries/Create
    public IActionResult Create() => View(new CreateBeneficiaryViewModel());

    // ── Create POST ──────────────────────────────────────────────
    // POST /Beneficiaries/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBeneficiaryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? token = User.FindFirst("token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        bool success = await _beneficiaryService.CreateAsync(model, token);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Could not save beneficiary. Check that the IBAN is valid and not already saved.");
            return View(model);
        }

        TempData["Success"] = $"{model.Name} was added to your beneficiaries.";
        return RedirectToAction(nameof(Index));
    }

    // ── Edit GET ─────────────────────────────────────────────────
    // GET /Beneficiaries/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        string? token = User.FindFirst("token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        EditBeneficiaryViewModel? model = await _beneficiaryService.GetByIdAsync(id, token);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    // ── Edit POST ────────────────────────────────────────────────
    // POST /Beneficiaries/Edit/{id}
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

        string? token = User.FindFirst("token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        bool success = await _beneficiaryService.UpdateAsync(model, token);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Could not update beneficiary. The IBAN may be invalid.");
            return View(model);
        }

        TempData["Success"] = $"{model.Name} was updated.";
        return RedirectToAction(nameof(Index));
    }

    // ── Delete POST ──────────────────────────────────────────────
    // POST /Beneficiaries/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        string? token = User.FindFirst("token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        bool success = await _beneficiaryService.DeleteAsync(id, token);

        TempData[success ? "Success" : "Error"] = success
            ? "Beneficiary removed."
            : "Could not remove beneficiary.";

        return RedirectToAction(nameof(Index));
    }
}