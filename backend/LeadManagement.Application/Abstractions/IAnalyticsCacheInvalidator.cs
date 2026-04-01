namespace LeadManagement.Application.Abstractions;

public interface IAnalyticsCacheInvalidator
{
    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
