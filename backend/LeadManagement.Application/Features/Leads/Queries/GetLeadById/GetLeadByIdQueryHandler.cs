using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Common.Authorization;
using LeadManagement.Application.Common.Mapping;
using LeadManagement.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Application.Features.Leads.Queries.GetLeadById;

public class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, LeadDetailDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly ILogger<GetLeadByIdQueryHandler> _logger;

    public GetLeadByIdQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService user,
        ILogger<GetLeadByIdQueryHandler> logger)
    {
        _db = db;
        _user = user;
        _logger = logger;
    }

    public async Task<LeadDetailDto?> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await _db.Leads
            .AsNoTracking()
            .Include(l => l.AssignedSalesRep)
            .Include(l => l.Interactions)
            .FirstOrDefaultAsync(l => l.LeadId == request.LeadId, cancellationToken);

        if (lead is null)
            return null;

        if (!LeadAccess.CanView(_user.Role, _user.SalesRepId, lead))
        {
            _logger.LogWarning("User {UserId} denied access to lead {LeadId}", _user.UserId, request.LeadId);
            throw new UnauthorizedAccessException("You do not have access to this lead.");
        }

        var dto = lead.ToDto();
        return new LeadDetailDto
        {
            Id = dto.Id,
            LeadId = dto.LeadId,
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Company = dto.Company,
            Position = dto.Position,
            Status = dto.Status,
            Source = dto.Source,
            Priority = dto.Priority,
            AssignedSalesRepId = dto.AssignedSalesRepId,
            AssignedTo = dto.AssignedTo,
            CreatedDate = dto.CreatedDate,
            ModifiedDate = dto.ModifiedDate,
            Interactions = lead.Interactions.OrderByDescending(i => i.InteractionDate).Select(i => i.ToDto()).ToList()
        };
    }
}
