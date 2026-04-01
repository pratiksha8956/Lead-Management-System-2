using LeadManagement.Application.Contracts;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;

namespace LeadManagement.Application.Common.Mapping;

public static class LeadMapper
{
    public static LeadDto ToDto(this Lead lead)
    {
        return new LeadDto
        {
            Id = lead.LeadId,
            LeadId = lead.LeadId,
            Name = lead.Name,
            Email = lead.Email,
            Phone = lead.Phone,
            Company = lead.Company,
            Position = lead.Position,
            Status = lead.Status.ToString(),
            Source = lead.Source,
            Priority = lead.Priority,
            AssignedSalesRepId = lead.AssignedSalesRepId,
            AssignedTo = lead.AssignedSalesRep?.Name ?? string.Empty,
            CreatedDate = lead.CreatedDate,
            ModifiedDate = lead.ModifiedDate
        };
    }

    public static InteractionDto ToDto(this Interaction interaction)
    {
        return new InteractionDto
        {
            InteractionId = interaction.InteractionId,
            LeadId = interaction.LeadId,
            Notes = interaction.Notes,
            InteractionDate = interaction.InteractionDate,
            FollowUpDate = interaction.FollowUpDate,
            CreatedDate = interaction.CreatedDate
        };
    }

    public static LeadStatus ParseStatus(string? value) =>
        Enum.TryParse<LeadStatus>(value, true, out var s) ? s : LeadStatus.New;
}
