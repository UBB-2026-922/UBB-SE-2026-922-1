// <copyright file="BillPaymentService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BillPaymentService class.
// </summary>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;

namespace BankingApp.Application.Services.BillPayments;

/// <summary>
/// Implements the business logic for bill payments.
/// </summary>
public class BillPaymentService : IBillPaymentService
{
    private const decimal SmallPaymentThreshold = 100m;
    private const decimal SmallPaymentFee = 0.50m;
    private const decimal StandardPaymentFee = 1.00m;
    private const int ReceiptUniqueSuffixLength = 6;

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
    public async Task<BillPayment> ProcessPaymentAsync(BillPaymentDto request)
    {
        var biller = await _billRepository.GetBillerByIdAsync(request.BillerId);
        if (biller == null)
        {
            throw new Exception("Biller not found.");
        }

        var account = await _billRepository.GetAccountByIdAsync(request.SourceAccountId);
        if (account == null)
        {
            throw new Exception("Source account not found.");
        }

        // Calculate fee based on amount
        decimal fee = CalculateFee(request.Amount);
        decimal totalAmount = request.Amount + fee;

        if (account.Balance < totalAmount)
        {
            throw new Exception("Insufficient funds to pay this bill (including fees).");
        }

        // Deduct money including fee
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
            TransactionRef = $"TXN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            Type = "BillPayment",
            Currency = account.Currency ?? "RON",
            BalanceAfter = account.Balance,
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
            Status = PaymentStatus.Completed,
            CreatedAt = DateTime.UtcNow,
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
    public async Task<bool> SaveBillerForUserAsync(int userId, int billerId, string nickname)
    {
        var saved = new SavedBiller
        {
            UserId = userId,
            BillerId = billerId,
            Nickname = nickname,
            CreatedAt = DateTime.UtcNow,
        };

        await _billRepository.AddSavedBillerAsync(saved);
        return true;
    }

    /// <summary>
    /// Works out the fee - up to 100 costs 0.50, above that costs 1.00.
    /// </summary>
    private decimal CalculateFee(decimal amount)
    {
        return amount <= SmallPaymentThreshold ? SmallPaymentFee : StandardPaymentFee;
    }

    /// <summary>
    /// Creates a unique receipt number like RCP-20260421-ABC123.
    /// </summary>
    private string GenerateReceiptNumber()
    {
        string uniqueSuffix = Guid.NewGuid().ToString("N")[..ReceiptUniqueSuffixLength].ToUpper();
        return $"RCP-{DateTime.UtcNow:yyyyMMdd}-{uniqueSuffix}";
    }
}