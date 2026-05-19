namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class RateAlerts
    {
        public const string Base = $"{Forex.Base}/rate-alerts";
        public const string ById = "{id:int}";

        public static string ByIdFull(int id) => $"{Base}/{id}";
    }
}
