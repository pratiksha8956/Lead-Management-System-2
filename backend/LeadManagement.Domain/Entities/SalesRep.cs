namespace LeadManagement.Domain.Entities;

public class SalesRep
{
    public int SalesRepId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
