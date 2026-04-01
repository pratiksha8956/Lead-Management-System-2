using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Common.Authorization;
using LeadManagement.Application.Common.Mapping;
using LeadManagement.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadManagement.Application.Features.Interactions.Queries.GetInteractions;

public class GetInteractionsQueryHandler : IRequestHandler<GetInteractionsQuery, IReadOnlyList<InteractionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public GetInteractionsQueryHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task<IReadOnlyList<InteractionDto>> Handle(GetInteractionsQuery request, CancellationToken cancellationToken)
    {
        var lead = await _db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LeadId == request.LeadId, cancellationToken);

        if (lead is null)
            throw new KeyNotFoundException("Lead not found.");

        if (!LeadAccess.CanView(_user.Role, _user.SalesRepId, lead))
            throw new UnauthorizedAccessException("You do not have access to this lead.");

        var list = await _db.Interactions
            .AsNoTracking()
            .Where(i => i.LeadId == request.LeadId)
            .OrderByDescending(i => i.InteractionDate)
            .ToListAsync(cancellationToken);

        return list.Select(i => i.ToDto()).ToList();
    }
}
