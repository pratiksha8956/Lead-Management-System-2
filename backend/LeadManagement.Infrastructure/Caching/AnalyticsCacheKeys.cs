namespace LeadManagement.Infrastructure.Caching;

public static class AnalyticsCacheKeys
{
    public const string Snapshot = "leadmgmt:analytics:snapshot";
    public static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);
}
