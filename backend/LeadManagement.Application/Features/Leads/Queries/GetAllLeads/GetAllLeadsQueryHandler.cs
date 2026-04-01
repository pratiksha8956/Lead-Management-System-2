using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Common.Mapping;
using LeadManagement.Application.Contracts;
using LeadManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Application.Features.Leads.Queries.GetAllLeads;

public class GetAllLeadsQueryHandler : IRequestHandler<GetAllLeadsQuery, PagedResult<LeadDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly ILogger<GetAllLeadsQueryHandler> _logger;

    public GetAllLeadsQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService user,
        ILogger<GetAllLeadsQueryHandler> logger)
    {
        _db = db;
        _user = user;
        _logger = logger;
    }

    public async Task<PagedResult<LeadDto>> Handle(GetAllLeadsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _db.Leads.AsNoTracking().Include(l => l.AssignedSalesRep).AsQueryable();

        if (_user.Role == UserRole.SalesRep)
        {
            if (!_user.SalesRepId.HasValue)
            {
                _logger.LogWarning("Sales rep {UserId} has no linked SalesRepId", _user.UserId);
                return new PagedResult<LeadDto>
                {
                    Items = Array.Empty<LeadDto>(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = 0,
                    TotalPages = 0
                };
            }

            query = query.Where(l => l.AssignedSalesRepId == _user.SalesRepId);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<LeadStatus>(request.Status, true, out var st))
            query = query.Where(l => l.Status == st);

        if (!string.IsNullOrWhiteSpace(request.Source))
            query = query.Where(l => l.Source == request.Source);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(l => l.ModifiedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResult<LeadDto>
        {
            Items = items.Select(l => l.ToDto()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = totalPages == 0 ? 1 : totalPages
        };
    }
}
