using LeadManagement.Application.Contracts;
using MediatR;

namespace LeadManagement.Application.Features.Leads.Commands.ConvertLead;

public record ConvertLeadCommand(int LeadId) : IRequest<LeadDto>;
