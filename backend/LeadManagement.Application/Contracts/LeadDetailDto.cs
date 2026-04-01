namespace LeadManagement.Application.Contracts;

public class LeadDetailDto : LeadDto
{
    public IReadOnlyList<InteractionDto> Interactions { get; init; } = Array.Empty<InteractionDto>();
}
