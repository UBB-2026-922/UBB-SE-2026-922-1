namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.Features.Beneficiaries.Dtos;
using Master;
using Services.Transfers;
using BankingApp.Application.Common.Utilities;
using Views;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     View model for the beneficiaries page in the desktop application.
///     Handles loading, adding and deleting beneficiaries and navigation interactions.
/// </summary>
public partial class BeneficiariesViewModel : ObservableObject
{
    private readonly ILogger<BeneficiariesViewModel> _logger;
    private readonly IAppNavigationService _navigationService;
    private readonly ITransferClientService _transferClientService;

    /// <summary>Initializes a new instance of the <see cref="BeneficiariesViewModel"/> class.</summary>
    public BeneficiariesViewModel(
        ITransferClientService transferClientService,
        IAppNavigationService navigationService,
        ILogger<BeneficiariesViewModel> logger)
    {
        _transferClientService = transferClientService ?? throw new ArgumentNullException(nameof(transferClientService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Beneficiaries = new List<BeneficiaryDto>();
    }

    /// <summary>Gets the currently loaded list of beneficiaries.</summary>
    public List<BeneficiaryDto> Beneficiaries { get; private set; }

    /// <summary>Gets or sets a value indicating whether the add-beneficiary form is visible.</summary>
    [ObservableProperty]
    public partial bool IsAddFormVisible { get; set; } = default!;

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
            ErrorOr<List<BeneficiaryDto>> result = await _transferClientService.GetBeneficiariesAsync();
            if (result.IsError)
            {
                ErrorMessage = "Failed to load beneficiaries.";
                return Error.Unauthorized();
            }

            Beneficiaries = result.Value;
            ErrorMessage = string.Empty;
            return Result.Success;
        }
        catch (Exception ex)
        {
            _logger.FailedToLoadBeneficiaries(ex);
            ErrorMessage = "An unexpected error occurred while loading beneficiaries.";
            return Error.Failure();
        }
    }

    /// <summary>Deletes the beneficiary with the specified id via the API.</summary>
    public async Task<bool> DeleteBeneficiaryAsync(int id)
    {
        try
        {
            ErrorOr<Success> result = await _transferClientService.DeleteBeneficiaryAsync(id);
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
            _logger.FailedToDeleteBeneficiary(ex, id);
            ErrorMessage = "An unexpected error occurred while deleting the beneficiary.";
            return false;
        }
    }

    /// <summary>Creates a new beneficiary using the values currently set on the view model.</summary>
    public async Task<bool> AddBeneficiaryAsync()
    {
        try
        {
            ErrorOr<Success> result = await _transferClientService.AddBeneficiaryAsync(NewName, NewIban, NewBankName);
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
            _logger.FailedToAddBeneficiary(exception);
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

        _navigationService.NavigateTo<NavView>();
    }
}
