using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Common.Mapping;
using LeadManagement.Application.Contracts;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Application.Features.Leads.Commands.CreateLead;

public class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, LeadDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly IAnalyticsCacheInvalidator _cacheInvalidator;
    private readonly ILogger<CreateLeadCommandHandler> _logger;

    public CreateLeadCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService user,
        IAnalyticsCacheInvalidator cacheInvalidator,
        ILogger<CreateLeadCommandHandler> logger)
    {
        _db = db;
        _user = user;
        _cacheInvalidator = cacheInvalidator;
        _logger = logger;
    }

    public async Task<LeadDto> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var emailNorm = request.Email.Trim().ToLowerInvariant();
        var exists = await _db.Leads.AnyAsync(l => l.Email.ToLower() == emailNorm, cancellationToken);
        if (exists)
            throw new InvalidOperationException("A lead with this email already exists.");

        int? assignedId = request.AssignedSalesRepId;
        if (_user.Role == UserRole.SalesRep)
            assignedId = _user.SalesRepId ?? throw new UnauthorizedAccessException("Sales rep profile is not linked.");
        else if (assignedId is null or 0)
            assignedId = null;

        if (assignedId.HasValue)
        {
            var repOk = await _db.SalesReps.AnyAsync(s => s.SalesRepId == assignedId.Value, cancellationToken);
            if (!repOk)
                throw new InvalidOperationException("Assigned sales rep was not found.");
        }

        var utc = DateTime.UtcNow;
        var lead = new Lead
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            Company = request.Company.Trim(),
            Position = request.Position.Trim(),
            Status = LeadStatus.New,
            Source = request.Source.Trim(),
            Priority = request.Priority.Trim(),
            AssignedSalesRepId = assignedId,
            CreatedDate = utc,
            ModifiedDate = utc
        };

        _db.Leads.Add(lead);
        await _db.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateAsync(cancellationToken);

        _logger.LogInformation("Lead {LeadId} created by user {UserId}", lead.LeadId, _user.UserId);

        var created = await _db.Leads
            .AsNoTracking()
            .Include(l => l.AssignedSalesRep)
            .FirstAsync(l => l.LeadId == lead.LeadId, cancellationToken);

        return created.ToDto();
    }
}
