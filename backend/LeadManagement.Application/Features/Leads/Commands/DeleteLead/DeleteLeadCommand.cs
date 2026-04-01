using MediatR;

namespace LeadManagement.Application.Features.Leads.Commands.DeleteLead;

public record DeleteLeadCommand(int LeadId) : IRequest<Unit>;
