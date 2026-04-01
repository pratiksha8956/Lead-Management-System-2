using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Common.Authorization;
using LeadManagement.Application.Common.Mapping;
using LeadManagement.Application.Contracts;
using LeadManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Application.Features.Leads.Commands.ConvertLead;

public class ConvertLeadCommandHandler : IRequestHandler<ConvertLeadCommand, LeadDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly IAnalyticsCacheInvalidator _cacheInvalidator;
    private readonly ILogger<ConvertLeadCommandHandler> _logger;

    public ConvertLeadCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService user,
        IAnalyticsCacheInvalidator cacheInvalidator,
        ILogger<ConvertLeadCommandHandler> logger)
    {
        _db = db;
        _user = user;
        _cacheInvalidator = cacheInvalidator;
        _logger = logger;
    }

    public async Task<LeadDto> Handle(ConvertLeadCommand request, CancellationToken cancellationToken)
    {
        if (!LeadAccess.CanConvert(_user.Role))
            throw new UnauthorizedAccessException("Only Admin or SalesManager can convert leads.");

        var lead = await _db.Leads
            .FirstOrDefaultAsync(l => l.LeadId == request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException("Lead not found.");

        if (!LeadAccess.CanView(_user.Role, _user.SalesRepId, lead))
            throw new UnauthorizedAccessException("You do not have access to this lead.");

        if (lead.Status != LeadStatus.Qualified)
            throw new InvalidOperationException("Only qualified leads can be converted.");

        lead.Status = LeadStatus.Converted;
        lead.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateAsync(cancellationToken);

        _logger.LogInformation("Lead {LeadId} converted by user {UserId}", lead.LeadId, _user.UserId);

        var updated = await _db.Leads
            .AsNoTracking()
            .Include(l => l.AssignedSalesRep)
            .FirstAsync(l => l.LeadId == lead.LeadId, cancellationToken);

        return updated.ToDto();
    }
}
