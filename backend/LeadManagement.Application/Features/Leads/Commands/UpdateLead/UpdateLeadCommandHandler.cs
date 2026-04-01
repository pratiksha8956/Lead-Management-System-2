using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Common.Authorization;
using LeadManagement.Application.Common.Mapping;
using LeadManagement.Application.Contracts;
using LeadManagement.Domain.Services;
using LeadManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Application.Features.Leads.Commands.UpdateLead;

public class UpdateLeadCommandHandler : IRequestHandler<UpdateLeadCommand, LeadDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly IAnalyticsCacheInvalidator _cacheInvalidator;
    private readonly ILogger<UpdateLeadCommandHandler> _logger;

    public UpdateLeadCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService user,
        IAnalyticsCacheInvalidator cacheInvalidator,
        ILogger<UpdateLeadCommandHandler> logger)
    {
        _db = db;
        _user = user;
        _cacheInvalidator = cacheInvalidator;
        _logger = logger;
    }

    public async Task<LeadDto> Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _db.Leads
            .FirstOrDefaultAsync(l => l.LeadId == request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException("Lead not found.");

        if (!LeadAccess.CanView(_user.Role, _user.SalesRepId, lead))
            throw new UnauthorizedAccessException("You do not have access to this lead.");

        if (LeadStatusTransition.IsConvertedReadOnly(lead.Status))
            throw new InvalidOperationException("Converted leads are read-only.");

        var nextStatus = LeadMapper.ParseStatus(request.Status);
        if (!LeadStatusTransition.TryValidateTransition(lead.Status, nextStatus))
            throw new InvalidOperationException($"Invalid status transition from {lead.Status} to {nextStatus}.");

        var emailNorm = request.Email.Trim().ToLowerInvariant();
        var emailTaken = await _db.Leads.AnyAsync(
            l => l.LeadId != request.LeadId && l.Email.ToLower() == emailNorm,
            cancellationToken);
        if (emailTaken)
            throw new InvalidOperationException("A lead with this email already exists.");

        int? assignedId = request.AssignedSalesRepId;
        if (_user.Role == UserRole.SalesRep)
            assignedId = lead.AssignedSalesRepId;
        else if (assignedId is 0)
            assignedId = null;

        if (assignedId.HasValue)
        {
            var repOk = await _db.SalesReps.AnyAsync(s => s.SalesRepId == assignedId.Value, cancellationToken);
            if (!repOk)
                throw new InvalidOperationException("Assigned sales rep was not found.");
        }

        lead.Name = request.Name.Trim();
        lead.Email = request.Email.Trim();
        lead.Phone = request.Phone.Trim();
        lead.Company = request.Company.Trim();
        lead.Position = request.Position.Trim();
        lead.Status = nextStatus;
        lead.Source = request.Source.Trim();
        lead.Priority = request.Priority.Trim();
        lead.AssignedSalesRepId = assignedId;
        lead.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateAsync(cancellationToken);

        _logger.LogInformation("Lead {LeadId} updated by user {UserId}", lead.LeadId, _user.UserId);

        var updated = await _db.Leads
            .AsNoTracking()
            .Include(l => l.AssignedSalesRep)
            .FirstAsync(l => l.LeadId == lead.LeadId, cancellationToken);

        return updated.ToDto();
    }
}
