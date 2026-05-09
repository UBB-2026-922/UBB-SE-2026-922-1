namespace BankingApp.Desktop.Tests.ViewModels;

using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Beneficiaries;
using Master;
using Services.Transfers;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public class BeneficiariesViewModelTests
{
    private readonly Mock<ITransferService> _transferService;
    private readonly BeneficiariesViewModel _viewModel;

    public BeneficiariesViewModelTests()
    {
        _transferService = new Mock<ITransferService>(MockBehavior.Strict);
        _viewModel = new BeneficiariesViewModel(_transferService.Object, Mock.Of<IAppNavigationService>(),
            NullLogger<BeneficiariesViewModel>.Instance);
    }

    [Fact]
    public async Task LoadBeneficiaries_WhenResponseIsValid_PopulatesViewModel()
    {
        var data = new List<BeneficiaryDto>
        {
            new BeneficiaryDto { Id = 1, Name = "Alice", Iban = "DE123", BankName = "Bank A" },
            new BeneficiaryDto { Id = 2, Name = "Bob", Iban = "DE456", BankName = "Bank B" }
        };

        _transferService
            .Setup(service => service.GetBeneficiariesAsync(default))
            .ReturnsAsync(data);

        ErrorOr<Success> result = await _viewModel.LoadBeneficiariesAsync();

        result.IsError.Should().BeFalse();
        _viewModel.Beneficiaries.Should().HaveCount(2);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadBeneficiaries_WhenUnauthorized_SetsErrorMessage()
    {
        _transferService
            .Setup(service => service.GetBeneficiariesAsync(default))
            .ReturnsAsync(Error.Unauthorized());

        ErrorOr<Success> result = await _viewModel.LoadBeneficiariesAsync();

        result.IsError.Should().BeTrue();
        _viewModel.Beneficiaries.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }
}
