namespace BankingApp.Infrastructure.Tests.Integration.TestSupport;

public abstract class IntegrationTestBase(SqlServerDatabaseFixture databaseFixture) : IAsyncLifetime
{
    protected SqlServerDatabaseFixture DatabaseFixture { get; } = databaseFixture;

    protected AppDbContext CreateDbContext()
    {
        return DatabaseFixture.CreateDbContext();
    }

    protected TestDbContextFactory CreateDbContextFactory()
    {
        return new TestDbContextFactory(DatabaseFixture);
    }

    protected TestServiceProviderFactory CreateServiceProviderFactory()
    {
        return new TestServiceProviderFactory(DatabaseFixture.ConnectionString);
    }

    protected TestDataSeeder CreateSeeder(AppDbContext dbContext)
    {
        return new TestDataSeeder(dbContext);
    }

    public virtual async ValueTask InitializeAsync()
    {
        await DatabaseFixture.ResetAsync();
    }

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
