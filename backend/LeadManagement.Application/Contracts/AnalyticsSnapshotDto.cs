namespace LeadManagement.Application.Contracts;

public class AnalyticsSnapshotDto
{
    public Dictionary<string, int> BySource { get; init; } = new();
    public Dictionary<string, int> ByStatus { get; init; } = new();
    public decimal ConversionRate { get; init; }
    public Dictionary<string, int> BySalesRep { get; init; } = new();
}
