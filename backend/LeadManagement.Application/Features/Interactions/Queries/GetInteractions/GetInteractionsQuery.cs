using LeadManagement.Application.Contracts;
using MediatR;

namespace LeadManagement.Application.Features.Interactions.Queries.GetInteractions;

public record GetInteractionsQuery(int LeadId) : IRequest<IReadOnlyList<InteractionDto>>;
