namespace BankingApp.Application.Tests.Services;


using BankingApp.Application.Features.Beneficiaries.Services;
using Domain.Entities;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
///     Unit tests for <see cref="BeneficiaryService" />.
/// </summary>
public class BeneficiaryServiceTests
{
    private const int DefaultUserId = 1;
    private const int DefaultBeneficiaryId = 10;
    private const string ValidIban = "RO49AAAA1B31007593840000";
    private const string InvalidIban = "INVALID123";
    private const string DefaultName = "John Doe";
    private const string EmptyName = "";

    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepository = new(MockBehavior.Strict);
    private readonly BeneficiaryService _service;

    /// <summary>
    ///     Initializes a new instance of the BeneficiaryServiceTests class.
    /// </summary>
    public BeneficiaryServiceTests()
    {
        _service = new BeneficiaryService(
            _beneficiaryRepository.Object,
            NullLogger<BeneficiaryService>.Instance);
    }

    [Fact]
    public void GetByUserId_WhenCalled_ReturnsBeneficiaries()
    {
        // Arrange
        List<Beneficiary> expectedBeneficiaries =
        [
            new() { Id = DefaultBeneficiaryId, UserId = DefaultUserId, Name = DefaultName, Iban = ValidIban }
        ];

        _beneficiaryRepository
            .Setup(findsByUserId => findsByUserId.FindByUserId(DefaultUserId))
            .Returns(expectedBeneficiaries);

        // Act
        ErrorOr<List<Beneficiary>> result = _service.GetByUserId(DefaultUserId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedBeneficiaries);
        _beneficiaryRepository.Verify(findsByUserId => findsByUserId.FindByUserId(DefaultUserId), Times.Once);
    }

    [Fact]
    public void ValidateIban_WhenIbanIsValid_ReturnsTrue()
    {
        // Act
        bool result = _service.ValidateIban(ValidIban);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateIban_WhenIbanIsInvalid_ReturnsFalse()
    {
        // Act
        bool result = _service.ValidateIban(InvalidIban);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Create_WhenIbanIsInvalid_ReturnsValidationError()
    {
        // Act
        ErrorOr<Beneficiary> result = _service.Create(DefaultUserId, DefaultName, InvalidIban, null);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Beneficiary.InvalidIban");
        _beneficiaryRepository.Verify(creates => creates.Create(It.IsAny<Beneficiary>()), Times.Never);
    }

    [Fact]
    public void Create_WhenNameIsEmpty_ReturnsValidationError()
    {
        // Act
        ErrorOr<Beneficiary> result = _service.Create(DefaultUserId, EmptyName, ValidIban, null);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Beneficiary.NameRequired");
        _beneficiaryRepository.Verify(creates => creates.Create(It.IsAny<Beneficiary>()), Times.Never);
    }

    [Fact]
    public void Create_WhenIbanAlreadyExists_ReturnsConflictError()
    {
        // Arrange
        _beneficiaryRepository
            .Setup(checksExists => checksExists.ExistsByUserIdAndIban(DefaultUserId, ValidIban))
            .Returns(true);

        // Act
        ErrorOr<Beneficiary> result = _service.Create(DefaultUserId, DefaultName, ValidIban, null);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Beneficiary.DuplicateIban");
        _beneficiaryRepository.Verify(creates => creates.Create(It.IsAny<Beneficiary>()), Times.Never);
    }

    [Fact]
    public void Create_WhenDataIsValid_SavesAndReturnsBeneficiary()
    {
        // Arrange
        const string bankName = " Bank Name ";
        _beneficiaryRepository
            .Setup(checksExists => checksExists.ExistsByUserIdAndIban(DefaultUserId, ValidIban))
            .Returns(false);
        _beneficiaryRepository
            .Setup(creates => creates.Create(It.IsAny<Beneficiary>()))
            .Returns((Beneficiary beneficiary) =>
            {
                beneficiary.Id = DefaultBeneficiaryId;
                return beneficiary;
            });

        var tolerance = TimeSpan.FromSeconds(2);

        // Act
        ErrorOr<Beneficiary> result = _service.Create(DefaultUserId, $" {DefaultName} ", ValidIban, bankName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(DefaultBeneficiaryId);
        result.Value.UserId.Should().Be(DefaultUserId);
        result.Value.Name.Should().Be(DefaultName);
        result.Value.Iban.Should().Be(ValidIban);
        result.Value.BankName.Should().Be("Bank Name");
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, tolerance);
        _beneficiaryRepository.Verify(
            creates => creates.Create(
                It.Is<Beneficiary>(beneficiary =>
                    beneficiary.UserId == DefaultUserId &&
                    beneficiary.Name == DefaultName &&
                    beneficiary.Iban == ValidIban &&
                    beneficiary.BankName == "Bank Name")),
            Times.Once);
    }

    [Fact]
    public void Update_WhenNameIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var beneficiary = new Beneficiary
        {
            Id = DefaultBeneficiaryId,
            UserId = DefaultUserId,
            Name = EmptyName,
            Iban = ValidIban
        };

        // Act
        ErrorOr<Success> result = _service.Update(beneficiary);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Beneficiary.NameRequired");
        _beneficiaryRepository.Verify(updates => updates.Update(It.IsAny<Beneficiary>()), Times.Never);
    }

    [Fact]
    public void Update_WhenDataIsValid_UpdatesExistingBeneficiary()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow.AddDays(-10);
        var existingBeneficiary = new Beneficiary
        {
            Id = DefaultBeneficiaryId,
            UserId = DefaultUserId,
            Name = "Old Name",
            Iban = "RO49AAAA1B31007593840001",
            BankName = "Old Bank",
            CreatedAt = createdAt,
            TotalAmountSent = 150,
            TransferCount = 3,
            LastTransferDate = DateTime.UtcNow.AddDays(-1)
        };
        var updatedBeneficiary = new Beneficiary
        {
            Id = DefaultBeneficiaryId,
            UserId = DefaultUserId,
            Name = $" {DefaultName} ",
            Iban = ValidIban,
            BankName = " New Bank "
        };

        _beneficiaryRepository
            .Setup(findsById => findsById.FindById(DefaultBeneficiaryId, DefaultUserId))
            .Returns(existingBeneficiary);
        _beneficiaryRepository
            .Setup(findsByUserId => findsByUserId.FindByUserId(DefaultUserId))
            .Returns((ErrorOr<List<Beneficiary>>)new List<Beneficiary> { existingBeneficiary });
        _beneficiaryRepository
            .Setup(updates => updates.Update(It.IsAny<Beneficiary>()))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.Update(updatedBeneficiary);

        // Assert
        result.IsError.Should().BeFalse();
        _beneficiaryRepository.Verify(
            updates => updates.Update(
                It.Is<Beneficiary>(beneficiary =>
                    beneficiary.Id == DefaultBeneficiaryId &&
                    beneficiary.UserId == DefaultUserId &&
                    beneficiary.Name == DefaultName &&
                    beneficiary.Iban == ValidIban &&
                    beneficiary.BankName == "New Bank" &&
                    beneficiary.CreatedAt == createdAt &&
                    beneficiary.TotalAmountSent == 150 &&
                    beneficiary.TransferCount == 3 &&
                    beneficiary.LastTransferDate == existingBeneficiary.LastTransferDate)),
            Times.Once);
    }

    [Fact]
    public void Delete_WhenCalled_DeletesBeneficiaryForUser()
    {
        // Arrange
        _beneficiaryRepository
            .Setup(deletes => deletes.Delete(DefaultBeneficiaryId, DefaultUserId))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.Delete(DefaultBeneficiaryId, DefaultUserId);

        // Assert
        result.IsError.Should().BeFalse();
        _beneficiaryRepository.Verify(
            deletes => deletes.Delete(DefaultBeneficiaryId, DefaultUserId),
            Times.Once);
    }
}
