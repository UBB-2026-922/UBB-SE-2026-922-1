namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Features.Beneficiaries.Dtos;
using Contracts.Features.Beneficiaries.Services;
using Views;
using ErrorOr;
using Logging;
using Microsoft.Extensions.Logging;
using Navigation;
using Shared;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>
///     View model for the beneficiaries page in the desktop application.
///     Handles loading, adding and deleting beneficiaries and navigation interactions.
/// </summary>
public partial class BeneficiariesViewModel : ObservableObject
{
    private readonly ILogger<BeneficiariesViewModel> _logger;
    private readonly IAppNavigationService _navigationService;
    private readonly IBeneficiaryService _beneficiaryService;

    /// <summary>Initializes a new instance of the <see cref="BeneficiariesViewModel"/> class.</summary>
    public BeneficiariesViewModel(
        IBeneficiaryService beneficiaryService,
        IAppNavigationService navigationService,
        ILogger<BeneficiariesViewModel> logger)
    {
        _beneficiaryService = beneficiaryService ?? throw new ArgumentNullException(nameof(beneficiaryService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Beneficiaries = [];
    }

    /// <summary>Gets the currently loaded list of beneficiaries.</summary>
    public List<BeneficiaryDto> Beneficiaries { get; private set; }

    /// <summary>Gets or sets a value indicating whether the add-beneficiary form is visible.</summary>
    [ObservableProperty]
    public partial bool IsAddFormVisible { get; set; } = false;

    /// <summary>Gets or sets the name entered for a new beneficiary.</summary>
    [ObservableProperty]
    public partial string NewName { get; set; } = string.Empty;

    /// <summary>Gets or sets the IBAN entered for a new beneficiary.</summary>
    [ObservableProperty]
    public partial string NewIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the bank name entered for a new beneficiary.</summary>
    [ObservableProperty]
    public partial string NewBankName { get; set; } = string.Empty;

    /// <summary>Gets or sets a human-readable error message to display when an operation fails.</summary>
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Loads the beneficiaries from the backend API.</summary>
    public async Task<ErrorOr<Success>> LoadBeneficiariesAsync()
    {
        try
        {
            ErrorOr<List<BeneficiaryDto>> result = await _beneficiaryService.GetAllAsync();
            if (result.IsError)
            {
                ErrorMessage = "Failed to load beneficiaries.";
                return result.FirstError;
            }

            Beneficiaries = result.Value;
            ErrorMessage = string.Empty;
            return Result.Success;
        }
        catch (Exception ex)
        {
            DesktopLogMessages.FailedToLoadBeneficiaries(_logger, ex);
            ErrorMessage = "An unexpected error occurred while loading beneficiaries.";
            return Error.Failure();
        }
    }

    /// <summary>Deletes the beneficiary with the specified id via the API.</summary>
    public async Task<bool> DeleteBeneficiaryAsync(int id)
    {
        try
        {
            ErrorOr<Success> result = await _beneficiaryService.DeleteAsync(id);
            if (result.IsError)
            {
                ErrorMessage = "Failed to delete beneficiary.";
                return false;
            }

            Beneficiaries.RemoveAll(beneficiary => beneficiary.Id == id);
            return true;
        }
        catch (Exception ex)
        {
            DesktopLogMessages.FailedToDeleteBeneficiary(_logger, ex, id);
            ErrorMessage = "An unexpected error occurred while deleting the beneficiary.";
            return false;
        }
    }

    /// <summary>Creates a new beneficiary using the values currently set on the view model.</summary>
    public async Task<bool> AddBeneficiaryAsync()
    {
        try
        {
            ErrorOr<Success> result = await _beneficiaryService.CreateAsync(new CreateBeneficiaryRequest
            {
                Name = NewName,
                Iban = NewIban,
                BankName = NewBankName,
            });
            if (result.IsError)
            {
                ErrorMessage = "Failed to save beneficiary.";
                return false;
            }

            await LoadBeneficiariesAsync();
            NewName = string.Empty;
            NewIban = string.Empty;
            NewBankName = string.Empty;
            IsAddFormVisible = false;
            return true;
        }
        catch (Exception exception)
        {
            DesktopLogMessages.FailedToAddBeneficiary(_logger, exception);
            ErrorMessage = "An unexpected error occurred while saving the beneficiary.";
            return false;
        }
    }

    /// <summary>Marks a beneficiary for use in a transfer and navigates to the transfer view.</summary>
    public void UseForTransfer(BeneficiaryDto? beneficiary)
    {
        if (beneficiary == null)
        {
            return;
        }

        _navigationService.NavigateTo<NavigationView>();
    }
}
