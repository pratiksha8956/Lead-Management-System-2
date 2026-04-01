using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Common.Authorization;
using LeadManagement.Application.Common.Mapping;
using LeadManagement.Application.Contracts;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Application.Features.Interactions.Commands.AddInteraction;

public class AddInteractionCommandHandler : IRequestHandler<AddInteractionCommand, InteractionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly ILogger<AddInteractionCommandHandler> _logger;

    public AddInteractionCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService user,
        ILogger<AddInteractionCommandHandler> logger)
    {
        _db = db;
        _user = user;
        _logger = logger;
    }

    public async Task<InteractionDto> Handle(AddInteractionCommand request, CancellationToken cancellationToken)
    {
        var lead = await _db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LeadId == request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException("Lead not found.");

        if (!LeadAccess.CanView(_user.Role, _user.SalesRepId, lead))
            throw new UnauthorizedAccessException("You do not have access to this lead.");

        if (lead.Status == LeadStatus.Converted)
            throw new InvalidOperationException("Cannot add interactions to converted leads.");

        var interactionDate = DateTime.SpecifyKind(request.InteractionDate, DateTimeKind.Utc);
        if (request.InteractionDate.Kind == DateTimeKind.Unspecified)
            interactionDate = DateTime.SpecifyKind(request.InteractionDate, DateTimeKind.Utc);

        if (interactionDate > DateTime.UtcNow.AddMinutes(1))
            throw new InvalidOperationException("Interaction date cannot be in the future.");

        DateTime? followUp = null;
        if (request.FollowUpDate.HasValue)
        {
            followUp = request.FollowUpDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.FollowUpDate.Value, DateTimeKind.Utc)
                : request.FollowUpDate.Value.ToUniversalTime();

            if (followUp <= interactionDate)
                throw new InvalidOperationException("Follow-up date must be after interaction date.");
        }

        var entity = new Interaction
        {
            LeadId = request.LeadId,
            Notes = request.Notes?.Trim() ?? string.Empty,
            InteractionDate = interactionDate,
            FollowUpDate = followUp,
            CreatedDate = DateTime.UtcNow
        };

        _db.Interactions.Add(entity);

        var trackedLead = await _db.Leads.FirstAsync(l => l.LeadId == request.LeadId, cancellationToken);
        trackedLead.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Interaction {InteractionId} added for lead {LeadId}", entity.InteractionId, request.LeadId);

        return entity.ToDto();
    }
}
