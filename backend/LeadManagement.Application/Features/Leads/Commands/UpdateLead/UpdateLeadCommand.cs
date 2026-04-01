using LeadManagement.Application.Contracts;
using MediatR;

namespace LeadManagement.Application.Features.Leads.Commands.UpdateLead;

public record UpdateLeadCommand(
    int LeadId,
    string Name,
    string Email,
    string Phone,
    string Company,
    string Position,
    string Status,
    string Source,
    string Priority,
    int? AssignedSalesRepId
) : IRequest<LeadDto>;
