namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class RecurringPayments
    {
        public const string Base = $"{ApiBase}/recurring_payments";

        public const string ById = "{id}";
        public const string Pause = "{id}/pause";
        public const string Resume = "{id}/resume";

        public static string ByIdFull(int id) => $"{Base}/{id}";
        public static string PauseFull(int id) => $"{Base}/{id}/pause";
        public static string ResumeFull(int id) => $"{Base}/{id}/resume";
    }
}
