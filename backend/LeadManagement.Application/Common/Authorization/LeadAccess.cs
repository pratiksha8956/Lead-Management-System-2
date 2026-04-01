using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;

namespace LeadManagement.Application.Common.Authorization;

public static class LeadAccess
{
    public static bool CanView(UserRole role, int? salesRepId, Lead lead)
    {
        if (role is UserRole.Admin or UserRole.SalesManager)
            return true;
        return salesRepId.HasValue && lead.AssignedSalesRepId == salesRepId;
    }

    public static bool CanConvert(UserRole role) =>
        role is UserRole.Admin or UserRole.SalesManager;
}
