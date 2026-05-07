namespace BankingApp.Infrastructure.Tests.Integration;

using Domain.Entities;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.DataAccess.Implementations;

using Infrastructure;
using Bogus;
using ErrorOr;
using Persistence;

/// <summary>
///     Integration tests for <see cref="BeneficiaryRepository" /> verifying repository
///     operations against the real database.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class BeneficiaryRepositoryTests : IAsyncLifetime
{
    private const int MissingBeneficiaryId = 99999;
    private const int ExpectedBeneficiaryCount = 2;
    private const int DefaultTransferCount = 0;
    private const int SeedTransferCount = 2;
    private const int UpdatedTransferCount = 4;
    private const decimal DefaultTotalAmountSent = 0m;
    private const decimal SeedTotalAmountSent = 500.25m;
    private const decimal UpdatedTotalAmountSent = 2500.75m;
    private const int InitialLastTransferDaysOffset = -2;
    private const int SeedLastTransferDaysOffset = -3;
    private const int UpdatedLastTransferDaysOffset = -1;
    private const int SeedCreatedAtDaysOffset = -10;

    private readonly DatabaseFixture _fixture;
    private readonly Faker<User> _userFaker;

    public BeneficiaryRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;

        _userFaker = new Faker<User>()
            .RuleFor(user => user.Email, faker => faker.Internet.Email())
            .RuleFor(user => user.PasswordHash, faker => faker.Internet.Password())
            .RuleFor(user => user.FullName, faker => faker.Person.FullName)
            .RuleFor(user => user.PreferredLanguage, _ => "en");
    }

    public ValueTask InitializeAsync()
    {
        return new ValueTask(_fixture.ResetAsync());
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public void Create_WhenBeneficiaryIsValid_PersistsAndReturnsEntity()
    {
        // Arrange
        using AppDatabaseContext databaseContext = CreateDatabaseContext();
        User beneficiaryOwner = SeedUser(databaseContext);
        BeneficiaryRepository beneficiaryRepository = new(databaseContext);
        var newBeneficiary = new Beneficiary
        {
            UserId = beneficiaryOwner.Id,
            Name = "Ava Recipient",
            Iban = "RO49AAAA1B31007593840000",
            BankName = "Transylvania Bank",
            LastTransferDate = DateTime.UtcNow.AddDays(InitialLastTransferDaysOffset),
            TotalAmountSent = DefaultTotalAmountSent,
            TransferCount = DefaultTransferCount,
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        ErrorOr<Beneficiary> createResult = beneficiaryRepository.Create(newBeneficiary);

        // Assert
        createResult.IsError.Should().BeFalse(createResult.IsError ? createResult.FirstError.Description : string.Empty);
        createResult.Value.Id.Should().BeGreaterThan(0);

        using AppDatabaseContext verificationContext = CreateDatabaseContext();
        Beneficiary? persistedBeneficiary = verificationContext.Beneficiaries
            .FirstOrDefault(beneficiary => beneficiary.Id == createResult.Value.Id);
        persistedBeneficiary.Should().NotBeNull();
        persistedBeneficiary.Name.Should().Be(newBeneficiary.Name);
        persistedBeneficiary.Iban.Should().Be(newBeneficiary.Iban);
    }

    [Fact]
    public void Create_WhenDuplicateIbanForUser_ReturnsError()
    {
        // Arrange
        using AppDatabaseContext databaseContext = CreateDatabaseContext();
        User beneficiaryOwner = SeedUser(databaseContext);
        BeneficiaryRepository beneficiaryRepository = new(databaseContext);
        string beneficiaryIban = "RO49AAAA1B31007593840000";
        var firstBeneficiary = new Beneficiary
        {
            UserId = beneficiaryOwner.Id,
            Name = "Sam Original",
            Iban = beneficiaryIban,
            BankName = "Transylvania Bank",
            CreatedAt = DateTime.UtcNow,
        };
        beneficiaryRepository.Create(firstBeneficiary).IsError.Should().BeFalse();

        var duplicateBeneficiary = new Beneficiary
        {
            UserId = beneficiaryOwner.Id,
            Name = "Sam Duplicate",
            Iban = beneficiaryIban,
            BankName = "Transylvania Bank",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        ErrorOr<Beneficiary> createResult = beneficiaryRepository.Create(duplicateBeneficiary);

        // Assert
        createResult.IsError.Should().BeTrue();
        createResult.FirstError.Code.Should().Be("Beneficiary.CreateFailed");
    }

    [Fact]
    public void FindById_WhenBeneficiaryExistsForUser_ReturnsBeneficiary()
    {
        // Arrange
        using AppDatabaseContext databaseContext = CreateDatabaseContext();
        User beneficiaryOwner = SeedUser(databaseContext);
        Beneficiary storedBeneficiary = SeedBeneficiary(
            databaseContext,
            beneficiaryOwner.Id,
            "Liam Beneficiary",
            "RO49AAAA1B31007593840001");
        BeneficiaryRepository beneficiaryRepository = new(databaseContext);

        // Act
        ErrorOr<Beneficiary> result = beneficiaryRepository.FindById(storedBeneficiary.Id, beneficiaryOwner.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Id.Should().Be(storedBeneficiary.Id);
        result.Value.UserId.Should().Be(beneficiaryOwner.Id);
        result.Value.Name.Should().Be(storedBeneficiary.Name);
        result.Value.Iban.Should().Be(storedBeneficiary.Iban);
    }

    [Fact]
    public void FindById_WhenBeneficiaryDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        using AppDatabaseContext databaseContext = CreateDatabaseContext();
        User beneficiaryOwner = SeedUser(databaseContext);
        BeneficiaryRepository beneficiaryRepository = new(databaseContext);

        // Act
        ErrorOr<Beneficiary> result = beneficiaryRepository.FindById(MissingBeneficiaryId, beneficiaryOwner.Id);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Beneficiary.NotFound");
    }

    [Fact]
    public void FindByUserId_WhenUserHasMultipleBeneficiaries_ReturnsOrderedByName()
    {
        // Arrange
        using AppDatabaseContext databaseContext = CreateDatabaseContext();
        User beneficiaryOwner = SeedUser(databaseContext);
        _ = SeedBeneficiary(databaseContext, beneficiaryOwner.Id, "Zara Example", "RO49AAAA1B31007593840002");
        _ = SeedBeneficiary(databaseContext, beneficiaryOwner.Id, "Alan Example", "RO49AAAA1B31007593840003");
        BeneficiaryRepository beneficiaryRepository = new(databaseContext);

        // Act
        ErrorOr<List<Beneficiary>> result = beneficiaryRepository.FindByUserId(beneficiaryOwner.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().HaveCount(ExpectedBeneficiaryCount);
        result.Value.Select(beneficiary => beneficiary.Name)
            .Should()
            .ContainInOrder("Alan Example", "Zara Example");
    }

    [Fact]
    public void ExistsByUserIdAndIban_WhenIbanMatchesIgnoringCaseAndWhitespace_ReturnsTrue()
    {
        // Arrange
        using AppDatabaseContext databaseContext = CreateDatabaseContext();
        User beneficiaryOwner = SeedUser(databaseContext);
        string beneficiaryIban = "RO49AAAA1B31007593840004";
        _ = SeedBeneficiary(databaseContext, beneficiaryOwner.Id, "Mia Recipient", beneficiaryIban);
        BeneficiaryRepository beneficiaryRepository = new(databaseContext);
        string inputIban = "  ro49aaaa1b31007593840004  ";

        // Act
        ErrorOr<bool> result = beneficiaryRepository.ExistsByUserIdAndIban(beneficiaryOwner.Id, inputIban);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void Update_WhenBeneficiaryExists_PersistsChanges()
    {
        // Arrange
        using AppDatabaseContext seedContext = CreateDatabaseContext();
        User beneficiaryOwner = SeedUser(seedContext);
        Beneficiary existingBeneficiary = SeedBeneficiary(
            seedContext,
            beneficiaryOwner.Id,
            "Noah Recipient",
            "RO49AAAA1B31007593840005");
        int existingBeneficiaryId = existingBeneficiary.Id;
        DateTime existingCreatedAt = existingBeneficiary.CreatedAt;
        string existingIban = existingBeneficiary.Iban;

        using AppDatabaseContext updateContext = CreateDatabaseContext();
        BeneficiaryRepository beneficiaryRepository = new(updateContext);
        DateTime updatedTransferDate = DateTime.UtcNow.AddDays(UpdatedLastTransferDaysOffset);
        var beneficiaryToUpdate = new Beneficiary
        {
            Id = existingBeneficiaryId,
            UserId = beneficiaryOwner.Id,
            Name = "Noah Updated",
            Iban = existingIban,
            BankName = "Updated Bank",
            LastTransferDate = updatedTransferDate,
            TotalAmountSent = UpdatedTotalAmountSent,
            TransferCount = UpdatedTransferCount,
            CreatedAt = existingCreatedAt,
        };

        // Act
        ErrorOr<Success> updateResult = beneficiaryRepository.Update(beneficiaryToUpdate);

        // Assert
        updateResult.IsError.Should().BeFalse(updateResult.IsError ? updateResult.FirstError.Description : string.Empty);

        using AppDatabaseContext verificationContext = CreateDatabaseContext();
        Beneficiary? updatedBeneficiary = verificationContext.Beneficiaries
            .FirstOrDefault(beneficiary => beneficiary.Id == existingBeneficiaryId);
        updatedBeneficiary.Should().NotBeNull();
        updatedBeneficiary.Name.Should().Be("Noah Updated");
        updatedBeneficiary.BankName.Should().Be("Updated Bank");
        updatedBeneficiary.LastTransferDate.Should().Be(updatedTransferDate);
        updatedBeneficiary.TotalAmountSent.Should().Be(UpdatedTotalAmountSent);
        updatedBeneficiary.TransferCount.Should().Be(UpdatedTransferCount);
    }

    [Fact]
    public void Delete_WhenBeneficiaryExists_RemovesRecord()
    {
        // Arrange
        using AppDatabaseContext databaseContext = CreateDatabaseContext();
        User beneficiaryOwner = SeedUser(databaseContext);
        Beneficiary existingBeneficiary = SeedBeneficiary(
            databaseContext,
            beneficiaryOwner.Id,
            "Olivia Recipient",
            "RO49AAAA1B31007593840006");
        BeneficiaryRepository beneficiaryRepository = new(databaseContext);

        // Act
        ErrorOr<Success> deleteResult = beneficiaryRepository.Delete(existingBeneficiary.Id, beneficiaryOwner.Id);

        // Assert
        deleteResult.IsError.Should().BeFalse(deleteResult.IsError ? deleteResult.FirstError.Description : string.Empty);

        using AppDatabaseContext verificationContext = CreateDatabaseContext();
        bool stillExists = verificationContext.Beneficiaries
            .Any(beneficiary => beneficiary.Id == existingBeneficiary.Id);
        stillExists.Should().BeFalse();
    }

    [Fact]
    public void Delete_WhenBeneficiaryDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        using AppDatabaseContext databaseContext = CreateDatabaseContext();
        User beneficiaryOwner = SeedUser(databaseContext);
        BeneficiaryRepository beneficiaryRepository = new(databaseContext);

        // Act
        ErrorOr<Success> result = beneficiaryRepository.Delete(MissingBeneficiaryId, beneficiaryOwner.Id);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Beneficiary.NotFound");
    }

    private static Beneficiary SeedBeneficiary(
        AppDatabaseContext databaseContext,
        int userId,
        string name,
        string beneficiaryIban)
    {
        var beneficiary = new Beneficiary
        {
            UserId = userId,
            Name = name,
            Iban = beneficiaryIban,
            BankName = "Seed Bank",
            LastTransferDate = DateTime.UtcNow.AddDays(SeedLastTransferDaysOffset),
            TotalAmountSent = SeedTotalAmountSent,
            TransferCount = SeedTransferCount,
            CreatedAt = DateTime.UtcNow.AddDays(SeedCreatedAtDaysOffset),
        };

        databaseContext.Beneficiaries.Add(beneficiary);
        databaseContext.SaveChanges();
        return beneficiary;
    }

    private AppDatabaseContext CreateDatabaseContext()
    {
        return _fixture.CreateDatabaseContext();
    }

    private User SeedUser(AppDatabaseContext databaseContext)
    {
        var userDataAccess = new UserDataAccess(databaseContext);
        User user = _userFaker.Generate();
        userDataAccess.Create(user).IsError.Should().BeFalse();

        ErrorOr<User> findResult = userDataAccess.FindByEmail(user.Email);
        findResult.IsError.Should().BeFalse(findResult.IsError ? findResult.FirstError.Description : string.Empty);
        return findResult.Value;
    }
}
