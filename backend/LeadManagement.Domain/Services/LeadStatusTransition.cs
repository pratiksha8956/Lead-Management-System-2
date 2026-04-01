using LeadManagement.Domain.Enums;

namespace LeadManagement.Domain.Services;

public static class LeadStatusTransition
{
    public static bool IsConvertedReadOnly(LeadStatus status) => status == LeadStatus.Converted;

    public static bool CanDelete(LeadStatus status) => status != LeadStatus.Converted;

    /// <summary>
    /// Validates status change rules: no skipping; Unqualified stays; Converted immutable; cannot set Converted via update (use convert endpoint).
    /// </summary>
    public static bool TryValidateTransition(LeadStatus from, LeadStatus to, bool allowSame = true)
    {
        if (from == LeadStatus.Converted)
            return allowSame && to == LeadStatus.Converted;

        if (to == LeadStatus.Converted)
            return false;

        if (from == to)
            return allowSame;

        return from switch
        {
            LeadStatus.New => to == LeadStatus.Contacted,
            LeadStatus.Contacted => to is LeadStatus.Qualified or LeadStatus.Unqualified,
            LeadStatus.Qualified => to is LeadStatus.Unqualified,
            LeadStatus.Unqualified => false,
            _ => false
        };
    }
}
