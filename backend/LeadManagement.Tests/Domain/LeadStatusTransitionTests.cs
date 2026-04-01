using LeadManagement.Domain.Enums;
using LeadManagement.Domain.Services;

namespace LeadManagement.Tests.Domain;

public class LeadStatusTransitionTests
{
    [Fact]
    public void New_May_Move_To_Contacted() =>
        Assert.True(LeadStatusTransition.TryValidateTransition(LeadStatus.New, LeadStatus.Contacted));

    [Fact]
    public void New_Cannot_Skip_To_Qualified() =>
        Assert.False(LeadStatusTransition.TryValidateTransition(LeadStatus.New, LeadStatus.Qualified));

    [Fact]
    public void New_Cannot_Skip_To_Unqualified() =>
        Assert.False(LeadStatusTransition.TryValidateTransition(LeadStatus.New, LeadStatus.Unqualified));

    [Fact]
    public void Contacted_May_Move_To_Qualified() =>
        Assert.True(LeadStatusTransition.TryValidateTransition(LeadStatus.Contacted, LeadStatus.Qualified));

    [Fact]
    public void Contacted_May_Move_To_Unqualified() =>
        Assert.True(LeadStatusTransition.TryValidateTransition(LeadStatus.Contacted, LeadStatus.Unqualified));

    [Fact]
    public void Contacted_Cannot_Return_To_New() =>
        Assert.False(LeadStatusTransition.TryValidateTransition(LeadStatus.Contacted, LeadStatus.New));

    [Fact]
    public void Qualified_May_Move_To_Unqualified() =>
        Assert.True(LeadStatusTransition.TryValidateTransition(LeadStatus.Qualified, LeadStatus.Unqualified));

    [Fact]
    public void Qualified_Cannot_Set_Converted_Via_Transition() =>
        Assert.False(LeadStatusTransition.TryValidateTransition(LeadStatus.Qualified, LeadStatus.Converted));

    [Fact]
    public void Unqualified_Is_Locked() =>
        Assert.False(LeadStatusTransition.TryValidateTransition(LeadStatus.Unqualified, LeadStatus.Contacted));

    [Fact]
    public void Converted_Is_ReadOnly_For_Status_Changes() =>
        Assert.False(LeadStatusTransition.TryValidateTransition(LeadStatus.Converted, LeadStatus.Qualified, allowSame: false));

    [Fact]
    public void Same_Status_Is_Allowed_When_Updating_NonStatus_Fields() =>
        Assert.True(LeadStatusTransition.TryValidateTransition(LeadStatus.Contacted, LeadStatus.Contacted));

    [Fact]
    public void Cannot_Delete_Converted() =>
        Assert.False(LeadStatusTransition.CanDelete(LeadStatus.Converted));

    [Fact]
    public void Can_Delete_Non_Converted() =>
        Assert.True(LeadStatusTransition.CanDelete(LeadStatus.New));

    [Fact]
    public void IsConvertedReadOnly_True_For_Converted() =>
        Assert.True(LeadStatusTransition.IsConvertedReadOnly(LeadStatus.Converted));
}
