using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Common.Authorization;
using LeadManagement.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Application.Features.Leads.Commands.DeleteLead;

public class DeleteLeadCommandHandler : IRequestHandler<DeleteLeadCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly IAnalyticsCacheInvalidator _cacheInvalidator;
    private readonly ILogger<DeleteLeadCommandHandler> _logger;

    public DeleteLeadCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService user,
        IAnalyticsCacheInvalidator cacheInvalidator,
        ILogger<DeleteLeadCommandHandler> logger)
    {
        _db = db;
        _user = user;
        _cacheInvalidator = cacheInvalidator;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _db.Leads
            .FirstOrDefaultAsync(l => l.LeadId == request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException("Lead not found.");

        if (!LeadAccess.CanView(_user.Role, _user.SalesRepId, lead))
            throw new UnauthorizedAccessException("You do not have access to this lead.");

        if (!LeadStatusTransition.CanDelete(lead.Status))
            throw new InvalidOperationException("Cannot delete a converted lead.");

        _db.Leads.Remove(lead);
        await _db.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateAsync(cancellationToken);

        _logger.LogInformation("Lead {LeadId} deleted by user {UserId}", request.LeadId, _user.UserId);
        return Unit.Value;
    }
}
