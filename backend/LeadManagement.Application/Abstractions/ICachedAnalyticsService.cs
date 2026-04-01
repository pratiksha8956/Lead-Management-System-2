using LeadManagement.Application.Contracts;

namespace LeadManagement.Application.Abstractions;

public interface ICachedAnalyticsService
{
    Task<AnalyticsSnapshotDto> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
