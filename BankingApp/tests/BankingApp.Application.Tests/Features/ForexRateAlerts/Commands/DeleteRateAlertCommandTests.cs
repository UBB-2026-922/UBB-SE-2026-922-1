namespace BankingApp.Application.Tests.Features.ForexRateAlerts.Commands;

using BankingApp.Application.Features.ForexRateAlerts.Commands;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Shared.Persistence;
using Currency = NodaMoney.Currency;

public sealed class DeleteRateAlertCommandTests
{
    private const int TestUserId = 1;
    private const int OtherUserId = 2;
    private const int TestAlertId = 100;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IRateAlertRepository> _rateAlertRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenRateAlertNotFound_ShouldReturnNotFoundErrorAndNotPersist()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _rateAlertRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestAlertId, cancellationToken))
            .ReturnsAsync((RateAlert?)null);

        DeleteRateAlertCommandHandler handler = CreateHandler();
        var command = new DeleteRateAlertCommand(TestUserId, TestAlertId);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RateAlertErrors.NotFound);

        _rateAlertRepositoryMock.Verify(repository => repository.GetByIdAsync(TestAlertId, cancellationToken), Times.Once);
        _rateAlertRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenRateAlertBelongsToDifferentUser_ShouldReturnNotFoundErrorAndNotPersist()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RateAlert otherUserAlert = CreateRateAlert(OtherUserId);

        _rateAlertRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestAlertId, cancellationToken))
            .ReturnsAsync(otherUserAlert);

        DeleteRateAlertCommandHandler handler = CreateHandler();
        var command = new DeleteRateAlertCommand(TestUserId, TestAlertId);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RateAlertErrors.NotFound);

        _rateAlertRepositoryMock.Verify(repository => repository.GetByIdAsync(TestAlertId, cancellationToken), Times.Once);
        _rateAlertRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldDeleteRateAlertAndSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RateAlert alert = CreateRateAlert(TestUserId);

        _rateAlertRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestAlertId, cancellationToken))
            .ReturnsAsync(alert);

        _rateAlertRepositoryMock
            .Setup(repository => repository.DeleteAsync(alert, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        DeleteRateAlertCommandHandler handler = CreateHandler();
        var command = new DeleteRateAlertCommand(TestUserId, TestAlertId);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);

        _rateAlertRepositoryMock.Verify(repository => repository.GetByIdAsync(TestAlertId, cancellationToken), Times.Once);
        _rateAlertRepositoryMock.Verify(repository => repository.DeleteAsync(alert, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _rateAlertRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    private DeleteRateAlertCommandHandler CreateHandler()
    {
        return new DeleteRateAlertCommandHandler(_rateAlertRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static RateAlert CreateRateAlert(int userId)
    {
        return RateAlert.Create(
            userId,
            Currency.FromCode("EUR"),
            Currency.FromCode("USD"),
            1.25m,
            true,
            _testNow).Value;
    }
}
