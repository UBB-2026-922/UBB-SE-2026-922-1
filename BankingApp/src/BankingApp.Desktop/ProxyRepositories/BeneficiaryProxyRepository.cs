namespace BankingApp.Desktop.ProxyRepositories;

using System;
using System.Collections.Generic;
using Application.Repositories.Interfaces;
using Domain.Entities;
using BankingApp.Desktop.Utilities;
using ErrorOr;

/// <summary>
///     Desktop-side proxy for <see cref="IBeneficiaryRepository" />.
/// </summary>
internal sealed class BeneficiaryProxyRepository : IBeneficiaryRepository
{
    private readonly IApiClient _apiClient;

    public BeneficiaryProxyRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ErrorOr<Beneficiary> FindById(int beneficiaryId, int userId)
        => _apiClient.GetAsync<Beneficiary>($"/api/beneficiaries/raw/{userId}/{beneficiaryId}").GetAwaiter().GetResult();

    public ErrorOr<List<Beneficiary>> FindByUserId(int userId)
        => _apiClient.GetAsync<List<Beneficiary>>($"/api/beneficiaries/raw/{userId}").GetAwaiter().GetResult();

    public ErrorOr<bool> ExistsByUserIdAndIban(int userId, string iban)
        => _apiClient.GetAsync<bool>($"/api/beneficiaries/raw/{userId}/exists?iban={Uri.EscapeDataString(iban)}").GetAwaiter().GetResult();

    public ErrorOr<Beneficiary> Create(Beneficiary beneficiary)
        => _apiClient.PostAsync<Beneficiary, Beneficiary>("/api/beneficiaries/raw", beneficiary).GetAwaiter().GetResult();

    public ErrorOr<Success> Update(Beneficiary beneficiary)
        => _apiClient.PutAsync("/api/beneficiaries/raw", beneficiary).GetAwaiter().GetResult();

    public ErrorOr<Success> Delete(int beneficiaryId, int userId)
        => _apiClient.DeleteAsync($"/api/beneficiaries/raw/{userId}/{beneficiaryId}").GetAwaiter().GetResult();
}
