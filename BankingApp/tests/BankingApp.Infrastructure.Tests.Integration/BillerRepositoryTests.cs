// Copyright (c) UBB-922. All rights reserved.
// Licensed under the MIT license.

using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.Repositories.Implementations;
using BankingApp.Infrastructure.Tests.Integration.Infrastructure;
using Bogus;
using ErrorOr;

namespace BankingApp.Infrastructure.Tests.Integration;

/// <summary>
///     Integration tests for <see cref="BillPaymentRepository" /> verifying biller
///     read operations are handled correctly against the real database.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class BillerRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly Faker<Biller> _billerFaker;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillerRepositoryTests" /> class.
    /// </summary>
    public BillerRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;

        _billerFaker = new Faker<Biller>()
            .RuleFor(biller => biller.Name, faker => faker.Company.CompanyName())
            .RuleFor(biller => biller.Category, _ => "Utilities")
            .RuleFor(biller => biller.IsActive, _ => true);
    }

    /// <summary>
    ///     Initializes the test fixture.
    /// </summary>
    public Task InitializeAsync()
    {
        return _fixture.ResetAsync();
    }

    /// <summary>
    ///     Disposes the test fixture.
    /// </summary>
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Verifies the GetBillersAsync_WhenBillersExist_ReturnsAllBillers scenario.
    /// </summary>
    [Fact]
    public async Task GetBillersAsync_WhenBillersExist_ReturnsAllBillers()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        BillPaymentRepository repository = MakeRepository(databaseContext);
        SeedBiller(databaseContext);
        SeedBiller(databaseContext);

        // Act
        IEnumerable<Biller> result = await repository.GetBillersAsync();

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    /// <summary>
    ///     Verifies the GetBillersAsync_WhenNoBillersExist_ReturnsEmptyList scenario.
    /// </summary>
    [Fact]
    public async Task GetBillersAsync_WhenNoBillersExist_ReturnsEmptyList()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        BillPaymentRepository repository = MakeRepository(databaseContext);

        // Act
        IEnumerable<Biller> result = await repository.GetBillersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies the GetBillerByIdAsync_WhenBillerExists_ReturnsBiller scenario.
    /// </summary>
    [Fact]
    public async Task GetBillerByIdAsync_WhenBillerExists_ReturnsBiller()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        BillPaymentRepository repository = MakeRepository(databaseContext);
        Biller biller = SeedBiller(databaseContext);

        // Act
        Biller? result = await repository.GetBillerByIdAsync(biller.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(biller.Id);
        result.Name.Should().Be(biller.Name);
    }

    /// <summary>
    ///     Verifies the GetBillerByIdAsync_WhenBillerDoesNotExist_ReturnsNull scenario.
    /// </summary>
    [Fact]
    public async Task GetBillerByIdAsync_WhenBillerDoesNotExist_ReturnsNull()
    {
        // Arrange
        using AppDatabaseContext databaseContext = _fixture.CreateDatabaseContext();
        BillPaymentRepository repository = MakeRepository(databaseContext);

        // Act
        Biller? result = await repository.GetBillerByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    private static BillPaymentRepository MakeRepository(AppDatabaseContext databaseContext)
    {
        return new BillPaymentRepository(databaseContext);
    }

    private Biller SeedBiller(AppDatabaseContext databaseContext)
    {
        Biller biller = _billerFaker.Generate();
        databaseContext.Billers.Add(biller);
        databaseContext.SaveChanges();
        return biller;
    }
}