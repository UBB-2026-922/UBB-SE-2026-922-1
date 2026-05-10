namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Application.DTOs.Transfer;
using Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Services;

/// <summary>
///     Provides data for the transfer history page.
///     Loads the authenticated user's past transfers from the API and exposes them
///     as pre-formatted <see cref="TransferHistoryDisplayItem" /> rows ready for binding.
/// </summary>
public partial class TransferHistoryViewModel : INotifyPropertyChanged
{
    private const string DateTimeFormat = "dd MMM yyyy, HH:mm";
    private const string FallbackReference = "—";

    private readonly ILogger<TransferHistoryViewModel> _logger;
    private readonly ITransferService _transferService;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferHistoryViewModel" /> class.
    /// </summary>
    /// <param name="transferService">The transfer service used to retrieve transfers.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public TransferHistoryViewModel(
        ITransferService transferService,
        ILogger<TransferHistoryViewModel> logger)
    {
        _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Transfers = new ObservableCollection<TransferHistoryDisplayItem>();
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Gets the pre-formatted transfer items shown in the list.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableCollection<TransferHistoryDisplayItem> Transfers { get; }

    /// <summary>
    ///     Gets or sets the error message shown when loading fails.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
                OnPropertyChanged(nameof(HasNoTransfers));
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether an error message is currently active.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    ///     Gets or sets a value indicating whether data is currently being fetched.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(HasNoTransfers));
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the list is empty and no error occurred —
    ///     used to show a "no transfers yet" placeholder.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool HasNoTransfers => !IsLoading && !HasError && Transfers.Count == 0;

    /// <summary>
    ///     Fetches the authenticated user's transfer history from the server and populates
    ///     <see cref="Transfers" /> with pre-formatted display rows.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous load operation.</returns>
    public async Task LoadHistoryAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        Transfers.Clear();

        try
        {
            ErrorOr<List<TransferResponse>> result =
                await _transferService.GetTransferHistoryAsync();

            if (result.IsError)
            {
                ErrorMessage = UserMessages.TransferHistory.LoadFailed;
                return;
            }

            foreach (TransferResponse transfer in result.Value)
            {
                Transfers.Add(MapToDisplayItem(transfer));
            }

            OnPropertyChanged(nameof(HasNoTransfers));
        }
        catch (Exception loadException)
        {
            _logger.LoadTransferHistoryFailedUnexpected(loadException);
            ErrorMessage = UserMessages.TransferHistory.LoadFailed;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Raises <see cref="PropertyChanged" /> for the given property.
    /// </summary>
    /// <param name="propertyName">The name of the changed property.</param>
    internal void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static TransferHistoryDisplayItem MapToDisplayItem(TransferResponse transfer)
    {
        string amountDisplay =
            $"-{transfer.Amount.ToString("F2", CultureInfo.InvariantCulture)} {transfer.Currency}";
        string dateDisplay =
            transfer.CreatedAt.ToLocalTime().ToString(DateTimeFormat, CultureInfo.CurrentCulture);
        string referenceDisplay = string.IsNullOrWhiteSpace(transfer.TransactionRef)
            ? FallbackReference
            : transfer.TransactionRef;

        return new TransferHistoryDisplayItem
        {
            RecipientName = transfer.RecipientName,
            RecipientIban = transfer.RecipientIban,
            BankName = transfer.RecipientBankName ?? string.Empty,
            AmountDisplay = amountDisplay,
            DateDisplay = dateDisplay,
            StatusDisplay = transfer.Status.ToString(),
            ReferenceDisplay = referenceDisplay,
        };
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
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
