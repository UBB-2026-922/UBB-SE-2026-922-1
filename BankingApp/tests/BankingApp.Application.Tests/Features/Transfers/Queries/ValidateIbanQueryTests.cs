namespace BankingApp.Application.Tests.Features.Transfers.Queries;

using BankingApp.Application.Features.Transfers.Queries;
using Contracts.Features.Transfers.Dtos;
using ErrorOr;

public sealed class ValidateIbanQueryTests
{
    [Fact]
    public async Task Handle_WhenIbanIsValid_ShouldReturnValidResponse()
    {
        // Arrange
        ValidateIbanQueryHandler handler = new();
        var query = new ValidateIbanQuery("RO12BANK1234567890123456");

        // Act
        ErrorOr<TransferIbanValidationResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsValid.Should().BeTrue();
        result.Value.BankName.Should().Be("Romanian Bank");
    }

    [Fact]
    public async Task Handle_WhenIbanIsInvalid_ShouldReturnInvalidResponse()
    {
        // Arrange
        ValidateIbanQueryHandler handler = new();
        var query = new ValidateIbanQuery("invalid-iban");

        // Act
        ErrorOr<TransferIbanValidationResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsValid.Should().BeFalse();
        result.Value.BankName.Should().BeEmpty();
    }
}
