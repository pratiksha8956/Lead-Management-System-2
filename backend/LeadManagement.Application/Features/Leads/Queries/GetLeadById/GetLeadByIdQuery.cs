using LeadManagement.Application.Contracts;
using MediatR;

namespace LeadManagement.Application.Features.Leads.Queries.GetLeadById;

public record GetLeadByIdQuery(int LeadId) : IRequest<LeadDetailDto?>;
