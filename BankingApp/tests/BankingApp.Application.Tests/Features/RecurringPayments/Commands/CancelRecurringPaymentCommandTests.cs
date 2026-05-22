namespace BankingApp.Application.Tests.Features.RecurringPayments.Commands;

using BankingApp.Application.Features.RecurringPayments.Commands;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Shared.Persistence;

public sealed class CancelRecurringPaymentCommandTests
{
    private const int TestUserId = 1;
    private const int OtherUserId = 2;
    private const int TestPaymentId = 100;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IRecurringPaymentRepository> _recurringPaymentRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _recurringPaymentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestPaymentId, cancellationToken))
            .ReturnsAsync((RecurringPayment?)null);

        CancelRecurringPaymentCommandHandler handler = CreateHandler();
        var command = new CancelRecurringPaymentCommand(TestUserId, TestPaymentId);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RecurringPaymentErrors.NotFound);

        _recurringPaymentRepositoryMock.Verify(repository => repository.GetByIdAsync(TestPaymentId, cancellationToken), Times.Once);
        _recurringPaymentRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenCalledByNonOwner_ShouldReturnNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RecurringPayment payment = CreatePayment(OtherUserId);

        _recurringPaymentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestPaymentId, cancellationToken))
            .ReturnsAsync(payment);

        CancelRecurringPaymentCommandHandler handler = CreateHandler();
        var command = new CancelRecurringPaymentCommand(TestUserId, TestPaymentId);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RecurringPaymentErrors.NotFound);

        _recurringPaymentRepositoryMock.Verify(repository => repository.GetByIdAsync(TestPaymentId, cancellationToken), Times.Once);
        _recurringPaymentRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCancelPaymentAndSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RecurringPayment payment = CreatePayment(TestUserId);

        _recurringPaymentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestPaymentId, cancellationToken))
            .ReturnsAsync(payment);

        _recurringPaymentRepositoryMock
            .Setup(repository => repository.UpdateAsync(payment, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        CancelRecurringPaymentCommandHandler handler = CreateHandler();
        var command = new CancelRecurringPaymentCommand(TestUserId, TestPaymentId);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
        payment.Status.Should().Be(RecurringPaymentStatus.Cancelled);

        _recurringPaymentRepositoryMock.Verify(repository => repository.GetByIdAsync(TestPaymentId, cancellationToken), Times.Once);
        _recurringPaymentRepositoryMock.Verify(repository => repository.UpdateAsync(payment, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _recurringPaymentRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    private CancelRecurringPaymentCommandHandler CreateHandler()
    {
        return new CancelRecurringPaymentCommandHandler(_recurringPaymentRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static RecurringPayment CreatePayment(int userId)
    {
        return RecurringPayment.Create(
            userId,
            10,
            20,
            50m,
            false,
            RecurringFrequency.Monthly,
            _testNow,
            null,
            _testNow).Value;
    }
}
