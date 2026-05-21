namespace BankingApp.Web.Controllers;

using Contracts.Features.ForexRateAlerts.Dtos;
using Contracts.Features.ForexRateAlerts.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.RateAlerts;

[Authorize]
public class RateAlertsController(IRateAlertService rateAlertService) : AuthenticatedController
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ErrorOr<List<ForexRateAlertDto>> result = await rateAlertService.GetAllAsync(ct);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load rate alerts. Please try again.";
            return View(new RateAlertListViewModel());
        }

        RateAlertListViewModel viewModel = new()
        {
            Alerts = result.Value.ConvertAll(a => new RateAlertRowViewModel
            {
                Id = a.Id,
                BaseCurrency = a.BaseCurrency,
                TargetCurrency = a.TargetCurrency,
                TargetRate = a.TargetRate,
                IsBuyAlert = a.IsBuyAlert,
                IsTriggered = a.IsTriggered,
                CreatedAt = a.CreatedAt,
            })
        };

        return View(viewModel);
    }

    public IActionResult Create() => View(new CreateRateAlertViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRateAlertViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (string.Equals(model.BaseCurrency, model.TargetCurrency, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Base and target currencies must be different.");
            return View(model);
        }

        ForexRateAlertDto dto = new()
        {
            BaseCurrency = model.BaseCurrency,
            TargetCurrency = model.TargetCurrency,
            TargetRate = model.TargetRate,
            IsBuyAlert = model.IsBuyAlert,
        };

        ErrorOr<ForexRateAlertDto> result = await rateAlertService.CreateAsync(dto, ct);

        if (result.IsError)
        {
            ModelState.AddModelError(string.Empty, "Could not create alert. Please try again.");
            return View(model);
        }

        TempData["Success"] = $"Rate alert created for {model.BaseCurrency}/{model.TargetCurrency} at {model.TargetRate:N6}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        ErrorOr<Success> result = await rateAlertService.DeleteAsync(id, ct);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? "Could not delete alert."
            : "Rate alert deleted.";

        return RedirectToAction(nameof(Index));
    }
}
