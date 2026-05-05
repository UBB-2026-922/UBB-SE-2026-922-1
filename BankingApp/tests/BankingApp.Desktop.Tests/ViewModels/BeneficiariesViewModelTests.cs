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

/// <summary>
///     Tests for <see cref="BeneficiariesViewModel" />.
/// </summary>
public class BeneficiariesViewModelTests
{
    private readonly Mock<IApiClient> _apiClient;
    private readonly BeneficiariesViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BeneficiariesViewModelTests" /> class.
    /// </summary>
    public BeneficiariesViewModelTests()
    {
        _apiClient = new Mock<IApiClient>(MockBehavior.Strict);
        _viewModel = new BeneficiariesViewModel(_apiClient.Object, Mock.Of<IAppNavigationService>(), NullLogger<BeneficiariesViewModel>.Instance);
    }

    /// <summary>
    ///     When the API returns a valid list, the view model should be populated with all beneficiaries.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadBeneficiaries_WhenResponseIsValid_PopulatesViewModel()
    {
        // Arrange
        var beneficiaries = new List<BeneficiaryDataTransferObject>
        {
            new BeneficiaryDataTransferObject { Id = 1, Name = "Alice", Iban = "DE123", BankName = "Bank A" },
            new BeneficiaryDataTransferObject { Id = 2, Name = "Bob", Iban = "DE456", BankName = "Bank B" },
        };

        _apiClient
            .Setup(getsAsync => getsAsync.GetAsync<List<BeneficiaryDataTransferObject>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiaries);

        // Act
        ErrorOr<Success> result = await _viewModel.LoadBeneficiariesAsync();

        // Assert
        result.IsError.Should().BeFalse();
        _viewModel.Beneficiaries.Should().HaveCount(2);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     When the API returns an Unauthorized error, the collection should remain empty and the error message should be set.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadBeneficiaries_WhenUnauthorized_SetsErrorMessage()
    {
        // Arrange
        _apiClient
            .Setup(getsAsync => getsAsync.GetAsync<List<BeneficiaryDataTransferObject>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unauthorized());

        // Act
        ErrorOr<Success> result = await _viewModel.LoadBeneficiariesAsync();

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.Beneficiaries.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }
}
