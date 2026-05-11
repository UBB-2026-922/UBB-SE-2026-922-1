namespace BankingApp.Infrastructure.Tests.Integration.TestSupport;

public sealed class TestDbContextFactory(SqlServerDatabaseFixture databaseFixture)
{
    public AppDbContext Create()
    {
        return databaseFixture.CreateDbContext();
    }
}
