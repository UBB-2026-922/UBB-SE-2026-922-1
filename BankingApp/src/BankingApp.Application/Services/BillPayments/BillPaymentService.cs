namespace BankingApp.Application.Services.BillPayments;

using System.Globalization;
using BankingApp.Application.DTOs.BillPayments;
using Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;

/// <summary>
/// Implements the business logic for bill payments.
/// </summary>
public class BillPaymentService : IBillPaymentService
{
    private const decimal SmallPaymentThreshold = 100m;
    private const decimal SmallPaymentFee = 0.50m;
    private const decimal StandardPaymentFee = 1.00m;
    private const int ReceiptUniqueSuffixLength = 6;
    private const decimal TwoFaAmountThreshold = 1000m;

    private readonly IBillPaymentRepository _billRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillPaymentService"/> class.
    /// </summary>
    /// <param name="billRepository">The bill payment repository.</param>
    public BillPaymentService(IBillPaymentRepository billRepository)
    {
        _billRepository = billRepository;
    }

    /// <inheritdoc/>
    public bool Requires2Fa(decimal amount)
    {
        return amount >= TwoFaAmountThreshold;
    }

    /// <inheritdoc/>
    public async Task<BillPayment> ProcessPaymentAsync(BillPaymentDto request)
    {
        Biller biller = await _billRepository.GetBillerByIdAsync(request.BillerId) ??
                        throw new KeyNotFoundException("Biller not found.");

        Account account = await _billRepository.GetAccountByIdAsync(request.SourceAccountId) ??
                           throw new KeyNotFoundException("Source account not found.");

        if (account.UserId != request.UserId)
        {
            throw new InvalidOperationException("Source account does not belong to the authenticated user.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.Amount,
                "Payment amount must be greater than zero.");
        }

        decimal fee = CalculateFee(request.Amount);
        decimal totalAmount = request.Amount + fee;

        if (account.Balance < totalAmount)
        {
            throw new InvalidOperationException("Insufficient funds to pay this bill (including fees).");
        }

        account.Balance -= totalAmount;
        await _billRepository.UpdateAccountAsync(account);

        var globalTransaction = new Transaction
        {
            AccountId = account.Id,
            CategoryId = null,
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
            BalanceAfter = account.Balance
        };

        await _billRepository.AddTransactionAsync(globalTransaction);

        var payment = new BillPayment
        {
            UserId = request.UserId,
            SourceAccountId = request.SourceAccountId,
            BillerId = request.BillerId,
            TransactionId = globalTransaction.Id,
            BillerReference = request.BillerReference,
            Amount = request.Amount,
            Fee = fee,
            ReceiptNumber = GenerateReceiptNumber(),
            Status = BillPaymentStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };

        await _billRepository.AddPaymentAsync(payment);

        return payment;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Biller>> GetAllBillersAsync()
    {
        return await _billRepository.GetBillersAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Account>> GetAccountsForUserAsync(int userId)
    {
        return await _billRepository.GetAccountsByUserIdAsync(userId);
    }

    /// <inheritdoc/>
    public async Task<bool> SaveBillerForUserAsync(int userId, int billerId, string nickname)
    {
        var saved = new SavedBiller
        {
            UserId = userId,
            BillerId = billerId,
            Nickname = nickname,
            CreatedAt = DateTime.UtcNow
        };

        await _billRepository.AddSavedBillerAsync(saved);
        return true;
    }

    /// <summary>
    /// Works out the fee - up to 100 costs 0.50, above that costs 1.00.
    /// </summary>
    public decimal CalculateFee(decimal amount)
    {
        return amount <= SmallPaymentThreshold ? SmallPaymentFee : StandardPaymentFee;
    }

    /// <summary>
    /// Creates a unique receipt number like RCP-20260421-ABC123.
    /// </summary>
    private static string GenerateReceiptNumber()
    {
        string uniqueSuffix = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)[..ReceiptUniqueSuffixLength]
            .ToUpperInvariant();
        return $"RCP-{DateTime.UtcNow:yyyyMMdd}-{uniqueSuffix}";
    }
}
