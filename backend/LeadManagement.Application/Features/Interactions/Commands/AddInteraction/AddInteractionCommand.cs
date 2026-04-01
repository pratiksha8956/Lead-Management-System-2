using LeadManagement.Application.Contracts;
using MediatR;

namespace LeadManagement.Application.Features.Interactions.Commands.AddInteraction;

public record AddInteractionCommand(int LeadId, string Notes, DateTime InteractionDate, DateTime? FollowUpDate)
    : IRequest<InteractionDto>;
