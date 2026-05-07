// <copyright file="BeneficiariesViewModelTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Beneficiaries;
using BankingApp.Desktop.Master;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BankingApp.Desktop.Tests.ViewModels;

public class BeneficiariesViewModelTests
{
    private readonly Mock<IApiClient> _apiClient;
    private readonly BeneficiariesViewModel _viewModel;

    public BeneficiariesViewModelTests()
    {
        _apiClient = new Mock<IApiClient>(MockBehavior.Strict);
        _viewModel = new BeneficiariesViewModel(_apiClient.Object, Mock.Of<IAppNavigationService>(),
            NullLogger<BeneficiariesViewModel>.Instance);
    }

    [Fact]
    public async Task LoadBeneficiaries_WhenResponseIsValid_PopulatesViewModel()
    {
        var data = new List<BeneficiaryDataTransferObject>
        {
            new BeneficiaryDataTransferObject { Id = 1, Name = "Alice", Iban = "DE123", BankName = "Bank A" },
            new BeneficiaryDataTransferObject { Id = 2, Name = "Bob", Iban = "DE456", BankName = "Bank B" }
        };

        _apiClient
            .Setup(gets =>
                gets.GetAsync<List<BeneficiaryDataTransferObject>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        ErrorOr<Success> result = await _viewModel.LoadBeneficiariesAsync();

        result.IsError.Should().BeFalse();
        _viewModel.Beneficiaries.Should().HaveCount(2);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadBeneficiaries_WhenUnauthorized_SetsErrorMessage()
    {
        _apiClient
            .Setup(gets =>
                gets.GetAsync<List<BeneficiaryDataTransferObject>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unauthorized());

        ErrorOr<Success> result = await _viewModel.LoadBeneficiariesAsync();

        result.IsError.Should().BeTrue();
        _viewModel.Beneficiaries.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }
}
