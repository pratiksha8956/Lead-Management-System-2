using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Contracts;
using LeadManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Application.Features.Analytics.Queries.GetAnalytics;

public class GetAnalyticsQueryHandler : IRequestHandler<GetAnalyticsQuery, AnalyticsSnapshotDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly ILogger<GetAnalyticsQueryHandler> _logger;

    public GetAnalyticsQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService user,
        ILogger<GetAnalyticsQueryHandler> logger)
    {
        _db = db;
        _user = user;
        _logger = logger;
    }

    public async Task<AnalyticsSnapshotDto> Handle(GetAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Leads.AsNoTracking().AsQueryable();

        if (_user.Role == UserRole.SalesRep)
        {
            if (!_user.SalesRepId.HasValue)
            {
                _logger.LogWarning("Analytics requested for sales rep without SalesRepId");
                return new AnalyticsSnapshotDto();
            }

            query = query.Where(l => l.AssignedSalesRepId == _user.SalesRepId);
        }

        var leads = await query
            .Select(l => new
            {
                l.Status,
                l.Source,
                RepName = l.AssignedSalesRep != null ? l.AssignedSalesRep.Name : "Unassigned"
            })
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Analytics computed over {Count} leads", leads.Count);

        var byStatus = leads
            .GroupBy(l => l.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var bySource = leads
            .GroupBy(l => string.IsNullOrWhiteSpace(l.Source) ? "Unknown" : l.Source)
            .ToDictionary(g => g.Key, g => g.Count());

        var byRep = leads
            .GroupBy(l => l.RepName)
            .ToDictionary(g => g.Key, g => g.Count());

        var total = leads.Count;
        var converted = leads.Count(l => l.Status == LeadStatus.Converted);
        var rate = total == 0 ? 0m : Math.Round((decimal)converted / total * 100m, 2);

        return new AnalyticsSnapshotDto
        {
            ByStatus = byStatus,
            BySource = bySource,
            BySalesRep = byRep,
            ConversionRate = rate
        };
    }
}
