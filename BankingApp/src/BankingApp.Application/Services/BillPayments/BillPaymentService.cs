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

        if (account.Balance < request.Amount)
        {
            throw new Exception("Insufficient funds to pay this bill.");
        }

        account.Balance -= request.Amount;
        await _billRepository.UpdateAccountAsync(account);

        var globalTransaction = new Transaction
        {
            AccountId = account.Id,
            CategoryId = null,
            Amount = request.Amount,
            Description = $"Bill Payment to {biller.Name} - Ref: {request.BillerReference}",
            CreatedAt = DateTime.UtcNow,
            Direction = TransactionDirection.Out,
            Status = TransactionStatus.Completed,
            TransactionRef = $"TXN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            Type = "BillPayment",
            Currency = account.Currency ?? "USD",
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
            Fee = 0,
            ReceiptNumber = $"REC-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
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
}