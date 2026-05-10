namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Beneficiaries;
using Master;
using Utilities;
using Views;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Services;

/// <summary>
///     View model for managing beneficiaries.
/// </summary>
public class BeneficiariesViewModel : System.ComponentModel.INotifyPropertyChanged
{
    private readonly ILogger<BeneficiariesViewModel> _logger;
    private readonly IAppNavigationService _navigationService;
    private readonly ITransferService _transferService;
    private List<BeneficiaryDto> _beneficiaries = new();
    private bool _isAddFormVisible;
    private string _newName = string.Empty;
    private string _newIban = string.Empty;
    private string _newBankName = string.Empty;
    private string _errorMessage = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BeneficiariesViewModel"/> class.
    /// </summary>
    /// <param name="transferService">The transfer service.</param>
    /// <param name="navigationService">The navigation service.</param>
    /// <param name="logger">The logger.</param>
    public BeneficiariesViewModel(ITransferService transferService, IAppNavigationService navigationService,
        ILogger<BeneficiariesViewModel> logger)
    {
        _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Gets the currently loaded list of beneficiaries.
    /// </summary>
    public List<BeneficiaryDto> Beneficiaries
    {
        get => _beneficiaries;
        private set => SetProperty(ref _beneficiaries, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the add-beneficiary form is visible.
    /// </summary>
    public bool IsAddFormVisible
    {
        get => _isAddFormVisible;
        set => SetProperty(ref _isAddFormVisible, value);
    }

    /// <summary>
    ///     Gets or sets the name entered for a new beneficiary.
    /// </summary>
    public string NewName
    {
        get => _newName;
        set => SetProperty(ref _newName, value);
    }

    /// <summary>
    ///     Gets or sets the IBAN entered for a new beneficiary.
    /// </summary>
    public string NewIban
    {
        get => _newIban;
        set => SetProperty(ref _newIban, value);
    }

    /// <summary>
    ///     Gets or sets the bank name entered for a new beneficiary.
    /// </summary>
    public string NewBankName
    {
        get => _newBankName;
        set => SetProperty(ref _newBankName, value);
    }

    /// <summary>
    ///     Gets the current user-facing error message.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    ///     Loads the beneficiaries from the backend API.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{Success}"/> indicating success or failure.</returns>
    public async Task<ErrorOr<Success>> LoadBeneficiariesAsync()
    {
        try
        {
            ErrorOr<List<BeneficiaryDto>> result =
                await _transferService.GetBeneficiariesAsync();

            if (result.IsError)
            {
                ErrorMessage = "Failed to load beneficiaries.";
                return Error.Unauthorized();
            }

            Beneficiaries = result.Value;
            ErrorMessage = string.Empty;
            return Result.Success;
        }
        catch (Exception exception)
        {
            _logger.FailedToLoadBeneficiaries(exception);
            ErrorMessage = "An unexpected error occurred while loading beneficiaries.";
            return Error.Failure();
        }
    }

    /// <summary>
    ///     Deletes the beneficiary with the specified id via the API.
    /// </summary>
    /// <param name="id">The beneficiary identifier.</param>
    /// <returns><see langword="true"/> when deletion succeeded; otherwise <see langword="false"/>.</returns>
    public async Task<bool> DeleteBeneficiaryAsync(int id)
    {
        try
        {
            ErrorOr<Success> result = await _transferService.DeleteBeneficiaryAsync(id);
            if (result.IsError)
            {
                ErrorMessage = "Failed to delete beneficiary.";
                return false;
            }

            var newList = new List<BeneficiaryDto>(Beneficiaries);
            newList.RemoveAll(beneficiary => beneficiary.Id == id);
            Beneficiaries = newList;
            return true;
        }
        catch (Exception exception)
        {
            _logger.FailedToDeleteBeneficiary(exception, id);
            ErrorMessage = "An unexpected error occurred while deleting the beneficiary.";
            return false;
        }
    }

    /// <summary>
    ///     Creates a new beneficiary using the values currently set on the view model.
    /// </summary>
    /// <returns><see langword="true"/> when creation succeeded; otherwise <see langword="false"/>.</returns>
    public async Task<bool> AddBeneficiaryAsync()
    {
        try
        {
            ErrorOr<Success> result = await _transferService.AddBeneficiaryAsync(NewName, NewIban, NewBankName);
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

    /// <summary>
    ///     Marks a beneficiary for use in a transfer and navigates to the transfer view.
    /// </summary>
    /// <param name="beneficiary">The beneficiary to use for a transfer.</param>
    public void UseForTransfer(BeneficiaryDto? beneficiary)
    {
        if (beneficiary == null)
        {
            return;
        }

        _navigationService.NavigateToContent<TransferView>();
    }

    /// <summary>
    ///     Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     Sets a property value and raises the <see cref="PropertyChanged"/> event if the value changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">A reference to the field to set.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns><see langword="true"/> if the value changed; otherwise <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
