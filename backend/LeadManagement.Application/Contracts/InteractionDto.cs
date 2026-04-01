namespace LeadManagement.Application.Contracts;

public class InteractionDto
{
    public int InteractionId { get; set; }
    public int LeadId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime InteractionDate { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
