// <copyright file="BillPaymentController.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the BillPaymentController class.
// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using BankingApp.Application.DTOs.BillPayment;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
///     Controller providing bill-payment endpoints used by the desktop client.
/// </summary>
[ApiController]
[Route("api/bill-payment")]
public class BillPaymentController : ControllerBase
{
    private const decimal SmallPaymentThreshold = 100m;
    private const decimal SmallPaymentFee = 0.50m;
    private const decimal StandardPaymentFee = 1.00m;
    private const decimal TwoFaAmountThreshold = 1000m;
    private const int ReceiptSuffixLength = 6;

    private static readonly List<BillerDto> MockBillers =
    [
        new() { Id = 1, Name = "Enel Energie", Category = "Utilities", IsActive = true },
        new() { Id = 2, Name = "E-ON Gaz", Category = "Utilities", IsActive = true },
        new() { Id = 3, Name = "Apa Nova", Category = "Utilities", IsActive = true },
        new() { Id = 4, Name = "Digi RCS-RDS", Category = "Internet", IsActive = true },
        new() { Id = 5, Name = "Orange Romania", Category = "Mobile", IsActive = true },
        new() { Id = 6, Name = "Vodafone Romania", Category = "Mobile", IsActive = true },
        new() { Id = 7, Name = "Allianz-Tiriac", Category = "Insurance", IsActive = true },
        new() { Id = 8, Name = "ANAF - Taxe", Category = "Taxes", IsActive = true },
        new() { Id = 9, Name = "Primaria Sector 1", Category = "Government", IsActive = true },
        new() { Id = 10, Name = "Netflix", Category = "Subscriptions", IsActive = true },
        new() { Id = 11, Name = "Telekom Romania", Category = "Telecom", IsActive = true },
        new() { Id = 12, Name = "Chiria Apartament", Category = "Rent", IsActive = true },
    ];

    private static readonly List<SavedBillerDto> MockSavedBillers =
    [
        new()
        {
            Id = 1, UserId = 1, BillerId = 1, Nickname = "Enel - Home",
            DefaultReference = "EL-2024-001234", CreatedAt = DateTime.UtcNow.AddDays(-30),
            Biller = MockBillers.First(b => b.Id == 1),
        },
        new()
        {
            Id = 2, UserId = 1, BillerId = 4, Nickname = "Digi Internet",
            DefaultReference = "DIGI-AB-99887766", CreatedAt = DateTime.UtcNow.AddDays(-15),
            Biller = MockBillers.First(b => b.Id == 4),
        },
    ];

    private static readonly List<AccountDto> MockAccounts =
    [
        new() { Id = 1, Iban = "RO49AAAA1B31007593840000", Currency = "EUR", Balance = 5000.00m, AccountName = "Main EUR Account", Status = "Active" },
        new() { Id = 2, Iban = "RO49AAAA1B31007593840001", Currency = "USD", Balance = 1200.00m, AccountName = "USD Account", Status = "Active" },
        new() { Id = 3, Iban = "RO49AAAA1B31007593840002", Currency = "RON", Balance = 8500.00m, AccountName = "RON Account", Status = "Active" },
        new() { Id = 4, Iban = "RO49AAAA1B31007593840003", Currency = "EUR", Balance = 300.00m, AccountName = "Savings EUR Account", Status = "Active" },
    ];

    /// <summary>Returns the biller directory, optionally filtered by category.</summary>
    [HttpGet("billers")]
    public ActionResult<List<BillerDto>> GetBillers([FromQuery] string? category)
    {
        var results = MockBillers.Where(b => b.IsActive);
        if (!string.IsNullOrWhiteSpace(category))
        {
            results = results.Where(b => string.Equals(b.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(results.ToList());
    }

    /// <summary>Searches billers by name and/or category.</summary>
    [HttpGet("billers/search")]
    public ActionResult<List<BillerDto>> SearchBillers([FromQuery] string? query, [FromQuery] string? category)
    {
        var results = MockBillers.Where(b => b.IsActive);
        if (!string.IsNullOrWhiteSpace(query))
        {
            results = results.Where(b => b.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            results = results.Where(b => string.Equals(b.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(results.ToList());
    }

    /// <summary>Returns saved billers for the authenticated user.</summary>
    [HttpGet("saved-billers")]
    public ActionResult<List<SavedBillerDto>> GetSavedBillers()
    {
        return Ok(MockSavedBillers);
    }

    /// <summary>Returns the available accounts for bill payment.</summary>
    [HttpGet("accounts")]
    public ActionResult<List<AccountDto>> GetAccounts()
    {
        return Ok(MockAccounts);
    }

    /// <summary>Calculates the payment fee for a given amount.</summary>
    [HttpGet("fee")]
    public ActionResult<FeeResponseDto> CalculateFee([FromQuery] decimal amount)
    {
        decimal fee = amount <= SmallPaymentThreshold ? SmallPaymentFee : StandardPaymentFee;
        return Ok(new FeeResponseDto { Fee = fee });
    }

    /// <summary>Checks whether 2FA is required for a given amount.</summary>
    [HttpGet("requires-2fa")]
    public ActionResult<Requires2FaResponseDto> Requires2Fa([FromQuery] decimal amount)
    {
        return Ok(new Requires2FaResponseDto { Required = amount >= TwoFaAmountThreshold });
    }

    /// <summary>Processes a bill payment and returns a receipt.</summary>
    [HttpPost("pay")]
    public ActionResult<BillPayResponseDto> PayBill([FromBody] BillPayRequestDto request)
    {
        decimal fee = request.Amount <= SmallPaymentThreshold ? SmallPaymentFee : StandardPaymentFee;
        string suffix = Guid.NewGuid().ToString("N")[..ReceiptSuffixLength].ToUpper();
        return Ok(new BillPayResponseDto
        {
            Id = Random.Shared.Next(1000, 9999),
            ReceiptNumber = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{suffix}",
            Fee = fee,
            Amount = request.Amount,
            Status = "Completed",
        });
    }

    /// <summary>Saves a biller to the user's favorites.</summary>
    [HttpPost("saved-billers")]
    public ActionResult<SavedBillerDto> SaveBiller([FromBody] SaveBillerRequestDto request)
    {
        var biller = MockBillers.FirstOrDefault(b => b.Id == request.BillerId);
        return Ok(new SavedBillerDto
        {
            Id = Random.Shared.Next(100, 999),
            UserId = 1,
            BillerId = request.BillerId,
            Nickname = request.Nickname,
            DefaultReference = request.DefaultReference,
            CreatedAt = DateTime.UtcNow,
            Biller = biller,
        });
    }
}
