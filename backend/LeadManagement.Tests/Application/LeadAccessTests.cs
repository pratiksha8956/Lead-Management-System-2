using LeadManagement.Application.Common.Authorization;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;

namespace LeadManagement.Tests.Application;

public class LeadAccessTests
{
    private static Lead LeadAssignedTo(int repId) => new()
    {
        LeadId = 1,
        AssignedSalesRepId = repId,
        Email = "x@test.com",
        Name = "X",
        Phone = "1",
        Company = "c",
        Position = "p",
        Source = "Website",
        Priority = "High"
    };

    [Fact]
    public void SalesRep_Can_View_Own_Lead() =>
        Assert.True(LeadAccess.CanView(UserRole.SalesRep, 5, LeadAssignedTo(5)));

    [Fact]
    public void SalesRep_Cannot_View_Other_Lead() =>
        Assert.False(LeadAccess.CanView(UserRole.SalesRep, 5, LeadAssignedTo(9)));

    [Fact]
    public void Manager_Can_View_Any_Lead() =>
        Assert.True(LeadAccess.CanView(UserRole.SalesManager, null, LeadAssignedTo(99)));

    [Fact]
    public void Admin_Can_View_Any_Lead() =>
        Assert.True(LeadAccess.CanView(UserRole.Admin, null, LeadAssignedTo(1)));

    [Fact]
    public void Only_Manager_And_Admin_Can_Convert() =>
        Assert.False(LeadAccess.CanConvert(UserRole.SalesRep));

    [Fact]
    public void Manager_Can_Convert() =>
        Assert.True(LeadAccess.CanConvert(UserRole.SalesManager));

    [Fact]
    public void Admin_Can_Convert() =>
        Assert.True(LeadAccess.CanConvert(UserRole.Admin));
}
