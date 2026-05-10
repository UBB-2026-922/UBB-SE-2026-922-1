namespace BankingApp.Desktop.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.BillPayments;
using Application.DTOs.Billers;
using Application.DTOs.RecurringPayments;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;
using Utilities;

/// <summary>
///     Implements <see cref="IBillPaymentClientService" /> with desktop-side business logic and proxy repositories.
/// </summary>
internal sealed class BillPaymentClientService(
    ICurrentSession currentSession,
    IBillerRepository billerRepository,
    IBillPaymentRepository billPaymentRepository,
    IRecurringPaymentRepository recurringPaymentRepository)
    : IBillPaymentClientService
{
    private const decimal SmallPaymentThreshold = 100m;
    private const decimal SmallPaymentFee = 0.50m;
    private const decimal StandardPaymentFee = 1.00m;
    private const decimal TwoFaAmountThreshold = 1000m;
    private const int ReceiptUniqueSuffixLength = 6;

    private readonly ICurrentSession _currentSession = currentSession ?? throw new ArgumentNullException(nameof(currentSession));
    private readonly IBillerRepository _billerRepository = billerRepository ?? throw new ArgumentNullException(nameof(billerRepository));
    private readonly IBillPaymentRepository _billPaymentRepository = billPaymentRepository ?? throw new ArgumentNullException(nameof(billPaymentRepository));
    private readonly IRecurringPaymentRepository _recurringPaymentRepository = recurringPaymentRepository ?? throw new ArgumentNullException(nameof(recurringPaymentRepository));

    public Task<ErrorOr<List<BillerDto>>> GetBillersAsync(string? search = null, string? category = null)
    {
        ErrorOr<List<Biller>> result = string.IsNullOrWhiteSpace(search) && string.IsNullOrWhiteSpace(category)
            ? _billerRepository.GetAllBillers()
            : _billerRepository.SearchBillers(search ?? string.Empty, category);

        return Task.FromResult<ErrorOr<List<BillerDto>>>(result.IsError
            ? result.FirstError
            : result.Value.ConvertAll(ToDto));
    }

    public Task<ErrorOr<List<SavedBillerDto>>> GetSavedBillersAsync()
    {
        int? userId = _currentSession.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<List<SavedBillerDto>>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<List<SavedBiller>> result = _billerRepository.GetSavedBillers(userId.Value);
        return Task.FromResult<ErrorOr<List<SavedBillerDto>>>(result.IsError
            ? result.FirstError
            : result.Value.ConvertAll(ToSavedDto));
    }

    public async Task<ErrorOr<List<AccountDto>>> GetAccountsAsync()
    {
        int? userId = _currentSession.GetCurrentUserId();
        if (userId is null)
        {
            return Error.Unauthorized(description: "User is not authenticated.");
        }

        IEnumerable<Account> accounts = await _billPaymentRepository.GetAccountsByUserIdAsync(userId.Value).ConfigureAwait(false);
        var mapped = accounts.Select(account => new AccountDto
        {
            Id = account.Id,
            Iban = account.Iban,
            Currency = account.Currency,
            Balance = account.Balance,
            AccountName = account.AccountName ?? string.Empty,
        }).ToList();

        return mapped;
    }

    public Task<ErrorOr<FeeResponse>> GetFeeAsync(decimal amount)
        => Task.FromResult<ErrorOr<FeeResponse>>(new FeeResponse { Fee = CalculateFee(amount) });

    public Task<ErrorOr<RequiresTwoFaResponse>> GetRequires2FaAsync(decimal amount)
        => Task.FromResult<ErrorOr<RequiresTwoFaResponse>>(new RequiresTwoFaResponse { Required = amount >= TwoFaAmountThreshold });

    public async Task<ErrorOr<BillPayResponse>> PayBillAsync(BillPayRequest request)
    {
        int? userId = _currentSession.GetCurrentUserId();
        if (userId is null)
        {
            return Error.Unauthorized(description: "User is not authenticated.");
        }

        Biller? biller = await _billPaymentRepository.GetBillerByIdAsync(request.BillerId).ConfigureAwait(false);
        if (biller is null)
        {
            return Error.NotFound(description: "Biller not found.");
        }

        Account? account = await _billPaymentRepository.GetAccountByIdAsync(request.SourceAccountId).ConfigureAwait(false);
        if (account is null)
        {
            return Error.NotFound(description: "Source account not found.");
        }

        if (account.User?.Id != userId.Value)
        {
            return Error.Forbidden(description: "Source account does not belong to the authenticated user.");
        }

        if (request.Amount <= 0)
        {
            return Error.Validation(description: "Payment amount must be greater than zero.");
        }

        decimal fee = CalculateFee(request.Amount);
        decimal totalAmount = request.Amount + fee;
        if (account.Balance < totalAmount)
        {
            return Error.Forbidden(description: "Insufficient funds to pay this bill (including fees).");
        }

        account.Balance -= totalAmount;
        await _billPaymentRepository.UpdateAccountAsync(account).ConfigureAwait(false);

        var globalTransaction = new Transaction
        {
            Account = account,
            Category = null,
            Amount = request.Amount,
            Fee = fee,
            Description = $"Bill Payment to {biller.Name} - Ref: {request.BillerReference}",
            CreatedAt = DateTime.UtcNow,
            Direction = TransactionDirection.Out,
            Status = TransactionStatus.Completed,
            TransactionRef = string.Create(
                CultureInfo.InvariantCulture,
                $"TXN-{Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)[..8].ToUpperInvariant()}"),
            Type = "BillPayment",
            Currency = account.Currency,
            BalanceAfter = account.Balance,
        };
        await _billPaymentRepository.AddTransactionAsync(globalTransaction).ConfigureAwait(false);

        var payment = new BillPayment
        {
            User = new User { Id = userId.Value },
            SourceAccount = account,
            Biller = biller,
            Transaction = globalTransaction,
            BillerReference = request.BillerReference,
            Amount = request.Amount,
            Fee = fee,
            ReceiptNumber = GenerateReceiptNumber(),
            Status = BillPaymentStatus.Completed,
            CreatedAt = DateTime.UtcNow,
        };
        await _billPaymentRepository.AddPaymentAsync(payment).ConfigureAwait(false);

        return new BillPayResponse
        {
            Id = payment.Id,
            ReceiptNumber = payment.ReceiptNumber,
            Fee = payment.Fee,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
        };
    }

    public Task<ErrorOr<SavedBillerDto>> SaveBillerAsync(SaveBillerRequest request)
    {
        int? userId = _currentSession.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<SavedBillerDto>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<Biller> billerResult = _billerRepository.GetBillerById(request.BillerId);
        if (billerResult.IsError)
        {
            return Task.FromResult<ErrorOr<SavedBillerDto>>(Error.NotFound(description: "Biller not found."));
        }

        ErrorOr<List<SavedBiller>> existingResult = _billerRepository.GetSavedBillers(userId.Value);
        if (!existingResult.IsError && existingResult.Value.Any(saved => saved.Biller?.Id == request.BillerId))
        {
            return Task.FromResult<ErrorOr<SavedBillerDto>>(Error.Conflict(description: "Biller already saved."));
        }

        var savedBiller = new SavedBiller
        {
            User = new User { Id = userId.Value },
            Biller = billerResult.Value,
            Nickname = request.Nickname,
            DefaultReference = request.DefaultReference,
            CreatedAt = DateTime.UtcNow,
        };

        ErrorOr<SavedBiller> saveResult = _billerRepository.SaveBiller(savedBiller);
        if (saveResult.IsError)
        {
            return Task.FromResult<ErrorOr<SavedBillerDto>>(saveResult.FirstError);
        }

        saveResult.Value.Biller = billerResult.Value;
        return Task.FromResult<ErrorOr<SavedBillerDto>>(ToSavedDto(saveResult.Value));
    }

    public Task<ErrorOr<List<RecurringPaymentResponse>>> GetRecurringPaymentsAsync()
    {
        int? userId = _currentSession.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<List<RecurringPaymentResponse>>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<List<RecurringPayment>> result = _recurringPaymentRepository.GetByUserId(userId.Value);
        return Task.FromResult<ErrorOr<List<RecurringPaymentResponse>>>(result.IsError
            ? result.FirstError
            : result.Value.ConvertAll(MapToRecurringResponse));
    }

    public Task<ErrorOr<RecurringPaymentResponse>> CreateRecurringPaymentAsync(CreateRecurringPaymentRequest request)
    {
        int? userId = _currentSession.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<RecurringPaymentResponse>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<RecurringPayment> paymentResult = RecurringPayment.Create(
            userId.Value,
            request.BillerId,
            request.SourceAccountId,
            request.Amount,
            request.IsPayInFull,
            request.Frequency,
            request.StartDate,
            request.EndDate,
            DateTime.UtcNow);
        if (paymentResult.IsError)
        {
            return Task.FromResult<ErrorOr<RecurringPaymentResponse>>(paymentResult.FirstError);
        }

        ErrorOr<RecurringPayment> createResult = _recurringPaymentRepository.Create(paymentResult.Value);
        return Task.FromResult<ErrorOr<RecurringPaymentResponse>>(createResult.IsError
            ? createResult.FirstError
            : MapToRecurringResponse(createResult.Value));
    }

    public Task<ErrorOr<Success>> PauseRecurringPaymentAsync(int paymentId)
        => ChangeRecurringPaymentState(paymentId, payment => payment.Pause(_currentSession.GetCurrentUserId() ?? 0));

    public Task<ErrorOr<Success>> ResumeRecurringPaymentAsync(int paymentId)
        => ChangeRecurringPaymentState(paymentId, payment => payment.Resume(_currentSession.GetCurrentUserId() ?? 0));

    public Task<ErrorOr<Success>> CancelRecurringPaymentAsync(int paymentId)
        => ChangeRecurringPaymentState(paymentId, payment => payment.Cancel(_currentSession.GetCurrentUserId() ?? 0));

    private Task<ErrorOr<Success>> ChangeRecurringPaymentState(
        int paymentId,
        Func<RecurringPayment, ErrorOr<Success>> transition)
    {
        ErrorOr<RecurringPayment> findResult = _recurringPaymentRepository.GetById(paymentId);
        if (findResult.IsError)
        {
            return Task.FromResult<ErrorOr<Success>>(findResult.FirstError);
        }

        ErrorOr<Success> transitionResult = transition(findResult.Value);
        if (transitionResult.IsError)
        {
            return Task.FromResult<ErrorOr<Success>>(transitionResult.FirstError);
        }

        return Task.FromResult(_recurringPaymentRepository.Update(findResult.Value));
    }

    private static decimal CalculateFee(decimal amount)
        => amount <= SmallPaymentThreshold ? SmallPaymentFee : StandardPaymentFee;

    private static string GenerateReceiptNumber()
    {
        string uniqueSuffix = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)[..ReceiptUniqueSuffixLength]
            .ToUpperInvariant();
        return $"RCP-{DateTime.UtcNow:yyyyMMdd}-{uniqueSuffix}";
    }

    private static BillerDto ToDto(Biller biller)
    {
        return new BillerDto
        {
            Id = biller.Id,
            Name = biller.Name,
            Category = biller.Category,
            LogoUrl = biller.LogoUrl,
            IsActive = biller.IsActive,
        };
    }

    private static SavedBillerDto ToSavedDto(SavedBiller savedBiller)
    {
        return new SavedBillerDto
        {
            Id = savedBiller.Id,
            UserId = savedBiller.User?.Id ?? 0,
            BillerId = savedBiller.Biller?.Id ?? 0,
            BillerName = savedBiller.Biller?.Name ?? string.Empty,
            BillerCategory = savedBiller.Biller?.Category ?? string.Empty,
            LogoUrl = savedBiller.Biller?.LogoUrl,
            Nickname = savedBiller.Nickname,
            DefaultReference = savedBiller.DefaultReference,
            CreatedAt = savedBiller.CreatedAt,
            Biller = savedBiller.Biller is null ? null : ToDto(savedBiller.Biller),
        };
    }

    private static RecurringPaymentResponse MapToRecurringResponse(RecurringPayment payment)
    {
        return new RecurringPaymentResponse
        {
            Id = payment.Id,
            UserId = payment.User?.Id ?? 0,
            BillerId = payment.Biller?.Id ?? 0,
            SourceAccountId = payment.SourceAccount?.Id ?? 0,
            Amount = payment.Amount,
            IsPayInFull = payment.IsPayInFull,
            Frequency = payment.Frequency,
            StartDate = payment.StartDate,
            EndDate = payment.EndDate,
            NextExecutionDate = payment.NextExecutionDate,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt,
        };
    }
}
