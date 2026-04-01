using System.Text;
using System.Text.Json;
using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Contracts;
using LeadManagement.Application.Features.Analytics.Queries.GetAnalytics;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Infrastructure.Caching;

public class CachedAnalyticsService : ICachedAnalyticsService
{
    private readonly IMediator _mediator;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedAnalyticsService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CachedAnalyticsService(
        IMediator mediator,
        IDistributedCache cache,
        ILogger<CachedAnalyticsService> logger)
    {
        _mediator = mediator;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AnalyticsSnapshotDto> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync(AnalyticsCacheKeys.Snapshot, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Analytics cache hit");
            return JsonSerializer.Deserialize<AnalyticsSnapshotDto>(cached, JsonOptions)
                   ?? new AnalyticsSnapshotDto();
        }

        _logger.LogDebug("Analytics cache miss; computing snapshot");
        var snapshot = await _mediator.Send(new GetAnalyticsQuery(), cancellationToken);
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(snapshot, JsonOptions));
        await _cache.SetAsync(
            AnalyticsCacheKeys.Snapshot,
            bytes,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = AnalyticsCacheKeys.Ttl
            },
            cancellationToken);

        return snapshot;
    }
}
