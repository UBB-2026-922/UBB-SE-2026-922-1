// <copyright file="BillPaymentRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the BillPaymentRepository class.
// </summary>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
/// Implements the data access operations for bill payments.
/// </summary>
public class BillPaymentRepository : IBillPaymentRepository
{
    private readonly AppDatabaseContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillPaymentRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public BillPaymentRepository(AppDatabaseContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Biller>> GetBillersAsync()
    {
        return await _context.Billers.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Biller?> GetBillerByIdAsync(int billerId)
    {
        return await _context.Billers.FirstOrDefaultAsync(b => b.Id == billerId);
    }

    /// <inheritdoc/>
    public async Task AddPaymentAsync(BillPayment payment)
    {
        await _context.BillPayments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BillPayment>> GetUserPaymentHistoryAsync(int userId)
    {
        return await _context.BillPayments
            .Where(bp => bp.UserId == userId)
            .OrderByDescending(bp => bp.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SavedBiller>> GetSavedBillersAsync(int userId)
    {
        return await _context.SavedBillers
            .Include(sb => sb.Biller)
            .Where(sb => sb.UserId == userId)
            .OrderByDescending(sb => sb.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task AddSavedBillerAsync(SavedBiller savedBiller)
    {
        await _context.SavedBillers.AddAsync(savedBiller);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<Account?> GetAccountByIdAsync(int accountId)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Account>> GetAccountsByUserIdAsync(int userId)
    {
        return await _context.Accounts
            .Where(account => account.UserId == userId)
            .OrderBy(account => account.AccountName)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAccountAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task AddTransactionAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }
}
