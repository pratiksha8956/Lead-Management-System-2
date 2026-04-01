using LeadManagement.Application.Contracts;
using MediatR;

namespace LeadManagement.Application.Features.Leads.Commands.CreateLead;

public record CreateLeadCommand(
    string Name,
    string Email,
    string Phone,
    string Company,
    string Position,
    string Source,
    string Priority,
    int? AssignedSalesRepId
) : IRequest<LeadDto>;
