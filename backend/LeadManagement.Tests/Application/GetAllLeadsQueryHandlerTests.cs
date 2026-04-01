using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Features.Leads.Queries.GetAllLeads;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
using LeadManagement.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LeadManagement.Tests.Application;

public class GetAllLeadsQueryHandlerTests
{
    [Fact]
    public async Task SalesRep_Sees_Only_Assigned_Leads()
    {
        await using var db = TestDbContextFactory.Create();
        db.SalesReps.AddRange(
            new SalesRep { Name = "R1", Email = "r1@test.com" },
            new SalesRep { Name = "R2", Email = "r2@test.com" });
        await db.SaveChangesAsync();
        var r1 = db.SalesReps.OrderBy(x => x.SalesRepId).First().SalesRepId;
        var r2 = db.SalesReps.OrderBy(x => x.SalesRepId).Last().SalesRepId;

        db.Leads.Add(new Lead
        {
            Name = "A",
            Email = "a@test.com",
            Phone = "1",
            Company = "c",
            Position = "p",
            Source = "Web",
            Priority = "Low",
            Status = LeadStatus.New,
            AssignedSalesRepId = r1
        });
        db.Leads.Add(new Lead
        {
            Name = "B",
            Email = "b@test.com",
            Phone = "1",
            Company = "c",
            Position = "p",
            Source = "Web",
            Priority = "Low",
            Status = LeadStatus.New,
            AssignedSalesRepId = r2
        });
        await db.SaveChangesAsync();

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.SalesRep);
        user.SetupGet(x => x.SalesRepId).Returns(r1);

        var handler = new GetAllLeadsQueryHandler(
            db,
            user.Object,
            NullLogger<GetAllLeadsQueryHandler>.Instance);

        var page = await handler.Handle(new GetAllLeadsQuery(1, 10, null, null), CancellationToken.None);
        Assert.Single(page.Items);
        Assert.Equal("A", page.Items[0].Name);
    }

    [Fact]
    public async Task Admin_Sees_All_Leads()
    {
        await using var db = TestDbContextFactory.Create();
        db.Leads.Add(new Lead
        {
            Name = "A",
            Email = "a@test.com",
            Phone = "1",
            Company = "c",
            Position = "p",
            Source = "Web",
            Priority = "Low"
        });
        db.Leads.Add(new Lead
        {
            Name = "B",
            Email = "b@test.com",
            Phone = "1",
            Company = "c",
            Position = "p",
            Source = "Web",
            Priority = "Low"
        });
        await db.SaveChangesAsync();

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.Admin);

        var handler = new GetAllLeadsQueryHandler(
            db,
            user.Object,
            NullLogger<GetAllLeadsQueryHandler>.Instance);

        var page = await handler.Handle(new GetAllLeadsQuery(1, 10, null, null), CancellationToken.None);
        Assert.Equal(2, page.Items.Count);
    }
}
