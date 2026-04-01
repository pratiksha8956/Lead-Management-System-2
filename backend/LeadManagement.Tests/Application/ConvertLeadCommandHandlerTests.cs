using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Features.Leads.Commands.ConvertLead;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
using LeadManagement.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LeadManagement.Tests.Application;

public class ConvertLeadCommandHandlerTests
{
    [Fact]
    public async Task SalesRep_Cannot_Convert()
    {
        await using var db = TestDbContextFactory.Create();
        db.Leads.Add(new Lead
        {
            Name = "X",
            Email = "x@test.com",
            Phone = "1",
            Company = "c",
            Position = "p",
            Source = "Web",
            Priority = "Low",
            Status = LeadStatus.Qualified,
            AssignedSalesRepId = 1
        });
        await db.SaveChangesAsync();
        var id = db.Leads.First().LeadId;

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.SalesRep);
        user.SetupGet(x => x.SalesRepId).Returns(1);
        user.SetupGet(x => x.UserId).Returns(1);

        var handler = new ConvertLeadCommandHandler(
            db,
            user.Object,
            Mock.Of<IAnalyticsCacheInvalidator>(),
            NullLogger<ConvertLeadCommandHandler>.Instance);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new ConvertLeadCommand(id), CancellationToken.None));
    }

    [Fact]
    public async Task Manager_Converts_Qualified_Lead()
    {
        await using var db = TestDbContextFactory.Create();
        db.Leads.Add(new Lead
        {
            Name = "X",
            Email = "x@test.com",
            Phone = "1",
            Company = "c",
            Position = "p",
            Source = "Web",
            Priority = "Low",
            Status = LeadStatus.Qualified
        });
        await db.SaveChangesAsync();
        var id = db.Leads.First().LeadId;

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.SalesManager);
        user.SetupGet(x => x.UserId).Returns(1);

        var handler = new ConvertLeadCommandHandler(
            db,
            user.Object,
            Mock.Of<IAnalyticsCacheInvalidator>(),
            NullLogger<ConvertLeadCommandHandler>.Instance);

        var dto = await handler.Handle(new ConvertLeadCommand(id), CancellationToken.None);
        Assert.Equal("Converted", dto.Status);
    }
}
