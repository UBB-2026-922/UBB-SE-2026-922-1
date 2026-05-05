// <copyright file="RecurringPaymentViewModelTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.TeamB;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using BankingApp.Domain.Enums;
using ErrorOr;
using FluentAssertions;
using Moq;
using Xunit;

namespace BankingApp.Desktop.Tests.ViewModels;

/// <summary>
///     Tests for the <see cref="RecurringPaymentViewModel" />.
/// </summary>
public class RecurringPaymentViewModelTests
{
    private readonly Mock<IApiClient> _apiClient;
    private readonly RecurringPaymentViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecurringPaymentViewModelTests" /> class.
    ///     Creates a fresh mock and view model for each test.
    /// </summary>
    public RecurringPaymentViewModelTests()
    {
        _apiClient = new Mock<IApiClient>(MockBehavior.Strict);
        _viewModel = new RecurringPaymentViewModel(_apiClient.Object);
    }

    /// <summary>
    ///     In LoadAsync, when all API responses are valid, the view model collections should be populated.
    /// </summary>
    [Fact]
    public async Task LoadAsync_WhenResponsesAreValid_PopulatesCollections()
    {
        // Arrange
        var accounts = new List<TransferAccountDto>
        {
            new TransferAccountDto { Id = 1, Name = "Checking", Balance = 1000m },
        };
        var payments = new List<RecurringPaymentDto>
        {
            new RecurringPaymentDto { Id = 1, Amount = 50m },
        };
        var billers = new List<BillerDto>
        {
            new BillerDto { Id = 1, Name = "Electric Co" },
        };

        _apiClient.Setup(api => api.GetAsync<List<TransferAccountDto>>(ApiEndpoints.TransferAccounts))
            .ReturnsAsync(accounts);
        _apiClient.Setup(api => api.GetAsync<List<RecurringPaymentDto>>(ApiEndpoints.RecurringPayments))
            .ReturnsAsync(payments);
        _apiClient.Setup(api => api.GetAsync<List<BillerDto>>(ApiEndpoints.Billers))
            .ReturnsAsync(billers);

        // Act
        await _viewModel.LoadAsync();

        // Assert
        _viewModel.Accounts.Should().BeEquivalentTo(accounts);
        _viewModel.Payments.Should().BeEquivalentTo(payments);
        _viewModel.Billers.Should().BeEquivalentTo(billers);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     In CreateAsync, when the inputs are valid, the payment should be created via the API and added to the collection.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WhenInputsAreValid_CreatesPaymentAndClearsForm()
    {
        // Arrange
        var biller = new BillerDto { Id = 1, Name = "Water Co" };
        var account = new TransferAccountDto { Id = 1, Name = "Checking" };
        var startDate = DateTime.Today;
        var amount = 100m;

        _viewModel.SelectedBiller = biller;
        _viewModel.SelectedAccount = account;
        _viewModel.Amount = amount;
        _viewModel.Frequency = RecurringFrequency.Monthly;
        _viewModel.StartDate = startDate;

        var expectedResult = new RecurringPaymentDto
        {
            Id = 123,
            BillerId = biller.Id,
            SourceAccountId = account.Id,
            Amount = amount,
            Frequency = RecurringFrequency.Monthly,
            StartDate = startDate,
        };

        _apiClient.Setup(api => api.PostAsync<RecurringPaymentDto, RecurringPaymentDto>(
                ApiEndpoints.RecurringPayments,
                It.Is<RecurringPaymentDto>(dto =>
                    dto.BillerId == biller.Id &&
                    dto.SourceAccountId == account.Id &&
                    dto.Amount == amount &&
                    dto.Frequency == RecurringFrequency.Monthly &&
                    dto.StartDate == startDate)))
            .ReturnsAsync(expectedResult);

        // Act
        await _viewModel.CreateAsync();

        // Assert
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.Payments.Should().Contain(expectedResult);
        _viewModel.SelectedBiller.Should().BeNull();
        _viewModel.SelectedAccount.Should().BeNull();
        _viewModel.Amount.Should().Be(0m);
    }

    /// <summary>
    ///     In PauseAsync, when a payment is selected, it should be paused via the API and its status updated in the collection.
    /// </summary>
    [Fact]
    public async Task PauseAsync_WhenPaymentIsSelected_PausesPayment()
    {
        // Arrange
        var payment = new RecurringPaymentDto { Id = 1, Status = RecurringPaymentStatus.Active };
        _viewModel.Payments.Add(payment);

        _apiClient.Setup(api => api.PutAsync<object, RecurringPaymentDto>(
                $"{ApiEndpoints.RecurringPayments}/{payment.Id}/pause",
                It.IsAny<object>()))
            .ReturnsAsync(payment);

        // Act
        await _viewModel.PauseAsync(payment);

        // Assert
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.Payments[0].Status.Should().Be(RecurringPaymentStatus.Paused);
    }

    /// <summary>
    ///     In ResumeAsync, when a payment is selected, it should be resumed via the API and its status updated in the collection.
    /// </summary>
    [Fact]
    public async Task ResumeAsync_WhenPaymentIsSelected_ResumesPayment()
    {
        // Arrange
        var payment = new RecurringPaymentDto { Id = 1, Status = RecurringPaymentStatus.Paused };
        _viewModel.Payments.Add(payment);

        _apiClient.Setup(api => api.PutAsync<object, RecurringPaymentDto>(
                $"{ApiEndpoints.RecurringPayments}/{payment.Id}/resume",
                It.IsAny<object>()))
            .ReturnsAsync(payment);

        // Act
        await _viewModel.ResumeAsync(payment);

        // Assert
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.Payments[0].Status.Should().Be(RecurringPaymentStatus.Active);
    }

    /// <summary>
    ///     In CancelAsync, when a payment is selected, it should be cancelled via the API and its status updated in the collection.
    /// </summary>
    [Fact]
    public async Task CancelAsync_WhenPaymentIsSelected_CancelsPayment()
    {
        // Arrange
        var payment = new RecurringPaymentDto { Id = 1, Status = RecurringPaymentStatus.Active };
        _viewModel.Payments.Add(payment);

        _apiClient.Setup(api => api.PutAsync<object, RecurringPaymentDto>(
                $"{ApiEndpoints.RecurringPayments}/{payment.Id}/cancel",
                It.IsAny<object>()))
            .ReturnsAsync(payment);

        // Act
        await _viewModel.CancelAsync(payment);

        // Assert
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.Payments[0].Status.Should().Be(RecurringPaymentStatus.Cancelled);
    }
}
