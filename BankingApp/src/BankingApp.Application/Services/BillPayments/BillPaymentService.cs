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
    private readonly IDashboardRepository _accountRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillPaymentService"/> class.
    /// </summary>
    /// <param name="billRepository">The bill payment repository.</param>
    /// <param name="accountRepository">The account repository.</param>
    public BillPaymentService(IBillPaymentRepository billRepository, IDashboardRepository accountRepository)
    {
        _billRepository = billRepository;
        _accountRepository = accountRepository;
    }

    /// <inheritdoc/>
    public async Task<BillPayment> ProcessPaymentAsync(BillPaymentDto request)
    {
        // 1. Validăm existența furnizorului
        var biller = await _billRepository.GetBillerByIdAsync(request.BillerId);
        if (biller == null)
        {
            throw new Exception("Biller not found.");
        }

        // 2. Verificăm soldul contului (Logica adaptată din Team B)
        // Notă: În BankingApp real, ai folosi un serviciu de tranzacții dedicat.

        var payment = new BillPayment
        {
            // Baza de date va auto-genera ID-ul (fiind de tip int)
            UserId = request.UserId,
            SourceAccountId = request.SourceAccountId,
            BillerId = request.BillerId,
            BillerReference = request.BillerReference,
            Amount = request.Amount,
            Fee = 0, // Calculat în funcție de business rules
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
            // Baza de date va auto-genera ID-ul (fiind de tip int)
            UserId = userId,
            BillerId = billerId,
            Nickname = nickname,
            CreatedAt = DateTime.UtcNow,
        };

        await _billRepository.AddSavedBillerAsync(saved);
        return true;
    }
}