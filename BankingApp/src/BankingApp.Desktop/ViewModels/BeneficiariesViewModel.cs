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
public class BeneficiariesViewModel
{
    private readonly ILogger<BeneficiariesViewModel> _logger;
    private readonly IAppNavigationService _navigationService;
    private readonly ITransferClientService _transferClientService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BeneficiariesViewModel"/> class.
    /// </summary>
    /// <param name="transferClientService">The transfer client service used to call backend endpoints.</param>
    /// <param name="navigationService">The navigation service used for view navigation.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public BeneficiariesViewModel(ITransferClientService transferClientService, IAppNavigationService navigationService,
        ILogger<BeneficiariesViewModel> logger)
    {
        _transferClientService = transferClientService ?? throw new ArgumentNullException(nameof(transferClientService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Beneficiaries = new List<BeneficiaryDto>();
        ErrorMessage = string.Empty;
    }

    /// <summary>
    ///     Gets the currently loaded list of beneficiaries.
    /// </summary>
    public List<BeneficiaryDto> Beneficiaries { get; private set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the add-beneficiary form is visible in the UI.
    /// </summary>
    public bool IsAddFormVisible { get; set; }

    /// <summary>Gets or sets the name entered for a new beneficiary.</summary>
    public string NewName { get; set; } = string.Empty;

    /// <summary>Gets or sets the IBAN entered for a new beneficiary.</summary>
    public string NewIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the bank name entered for a new beneficiary.</summary>
    public string NewBankName { get; set; } = string.Empty;

    /// <summary>Gets human-readable error message to display in the UI when an operation fails.</summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    ///     Loads the beneficiaries from the backend API into <see cref="Beneficiaries"/>.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{Success}"/> indicating success or failure.</returns>
    public async Task<ErrorOr<Success>> LoadBeneficiariesAsync()
    {
        try
        {
            ErrorOr<List<BeneficiaryDto>> result =
                await _transferClientService.GetBeneficiariesAsync();

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
    /// <param name="id">The beneficiary identifier.</param>
    /// <returns><see langword="true"/> when deletion succeeded; otherwise <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> when creation succeeded; otherwise <see langword="false"/>.</returns>
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

            // reload
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

    /// <summary>
    ///     Marks a beneficiary for use in a transfer and navigates to the transfer view (placeholder).
    /// </summary>
    /// <param name="beneficiary">The beneficiary to use for a transfer.</param>
    public void UseForTransfer(BeneficiaryDto? beneficiary)
    {
        if (beneficiary == null)
        {
            return;
        }

        // navigate to the main nav view as a placeholder for transfer navigation.
        _navigationService.NavigateTo<NavView>();
    }
}
