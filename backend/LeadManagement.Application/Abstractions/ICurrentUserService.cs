using LeadManagement.Domain.Enums;

namespace LeadManagement.Application.Abstractions;

public interface ICurrentUserService
{
    int UserId { get; }
    UserRole Role { get; }
    int? SalesRepId { get; }
    bool IsAuthenticated { get; }
}
