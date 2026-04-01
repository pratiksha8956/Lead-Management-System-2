using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Features.Analytics.Queries.GetAnalytics;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
using LeadManagement.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LeadManagement.Tests.Application;

public class GetAnalyticsQueryHandlerTests
{
    [Fact]
    public async Task Conversion_Rate_Uses_Converted_Over_Total()
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
            Priority = "Low",
            Status = LeadStatus.Converted
        });
        db.Leads.Add(new Lead
        {
            Name = "B",
            Email = "b@test.com",
            Phone = "1",
            Company = "c",
            Position = "p",
            Source = "Referral",
            Priority = "Low",
            Status = LeadStatus.New
        });
        await db.SaveChangesAsync();

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.Admin);
        user.SetupGet(x => x.SalesRepId).Returns((int?)null);

        var handler = new GetAnalyticsQueryHandler(
            db,
            user.Object,
            NullLogger<GetAnalyticsQueryHandler>.Instance);

        var snap = await handler.Handle(new GetAnalyticsQuery(), CancellationToken.None);
        Assert.Equal(50m, snap.ConversionRate);
        Assert.Equal(1, snap.ByStatus.GetValueOrDefault("Converted"));
        Assert.Equal(1, snap.BySource.GetValueOrDefault("Web"));
    }

    [Fact]
    public async Task SalesRep_Analytics_Only_Assigned_Leads()
    {
        await using var db = TestDbContextFactory.Create();
        db.SalesReps.Add(new SalesRep { Name = "Own", Email = "own@test.com" });
        await db.SaveChangesAsync();
        var repId = db.SalesReps.First().SalesRepId;

        db.Leads.Add(new Lead
        {
            Name = "Mine",
            Email = "m@test.com",
            Phone = "1",
            Company = "c",
            Position = "p",
            Source = "Web",
            Priority = "Low",
            Status = LeadStatus.New,
            AssignedSalesRepId = repId
        });
        db.Leads.Add(new Lead
        {
            Name = "Other",
            Email = "o@test.com",
            Phone = "1",
            Company = "c",
            Position = "p",
            Source = "Web",
            Priority = "Low",
            Status = LeadStatus.New,
            AssignedSalesRepId = null
        });
        await db.SaveChangesAsync();

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.SalesRep);
        user.SetupGet(x => x.SalesRepId).Returns(repId);

        var handler = new GetAnalyticsQueryHandler(
            db,
            user.Object,
            NullLogger<GetAnalyticsQueryHandler>.Instance);

        var snap = await handler.Handle(new GetAnalyticsQuery(), CancellationToken.None);
        Assert.Single(snap.ByStatus);
        Assert.Equal(1, snap.ByStatus["New"]);
    }
}
