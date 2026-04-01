using LeadManagement.Application.Contracts;
using MediatR;

namespace LeadManagement.Application.Features.Leads.Queries.GetAllLeads;

public record GetAllLeadsQuery(int Page, int PageSize, string? Status, string? Source)
    : IRequest<PagedResult<LeadDto>>;
