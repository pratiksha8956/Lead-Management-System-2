namespace LeadManagement.Domain.Entities;

public class Interaction
{
    public int InteractionId { get; set; }
    public int LeadId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime InteractionDate { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Lead Lead { get; set; } = null!;
}
