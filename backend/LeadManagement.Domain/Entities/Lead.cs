using LeadManagement.Domain.Enums;

namespace LeadManagement.Domain.Entities;

public class Lead
{
    public int LeadId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public string Source { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int? AssignedSalesRepId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    public SalesRep? AssignedSalesRep { get; set; }
    public ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
}
