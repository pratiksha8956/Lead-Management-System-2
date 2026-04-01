using LeadManagement.Domain.Enums;

namespace LeadManagement.Domain.Entities;

public class AppUser
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int? SalesRepId { get; set; }

    public SalesRep? SalesRep { get; set; }
}
