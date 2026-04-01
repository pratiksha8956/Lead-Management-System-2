using LeadManagement.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Infrastructure.Caching;

public class AnalyticsCacheInvalidator : IAnalyticsCacheInvalidator
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<AnalyticsCacheInvalidator> _logger;

    public AnalyticsCacheInvalidator(IDistributedCache cache, ILogger<AnalyticsCacheInvalidator> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Invalidating analytics cache");
        return _cache.RemoveAsync(AnalyticsCacheKeys.Snapshot, cancellationToken);
    }
}
