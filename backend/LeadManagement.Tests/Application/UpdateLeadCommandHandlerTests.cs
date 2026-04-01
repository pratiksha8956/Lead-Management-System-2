using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Features.Leads.Commands.UpdateLead;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
using LeadManagement.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LeadManagement.Tests.Application;

public class UpdateLeadCommandHandlerTests
{
    [Fact]
    public async Task Invalid_Transition_Throws()
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
            Status = LeadStatus.New
        });
        await db.SaveChangesAsync();
        var id = db.Leads.First().LeadId;

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.Admin);
        user.SetupGet(x => x.UserId).Returns(1);

        var handler = new UpdateLeadCommandHandler(
            db,
            user.Object,
            Mock.Of<IAnalyticsCacheInvalidator>(),
            NullLogger<UpdateLeadCommandHandler>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new UpdateLeadCommand(id, "X", "x@test.com", "1", "c", "p", "Qualified", "Web", "Low", null),
            CancellationToken.None));
    }

    [Fact]
    public async Task Valid_Transition_Updates()
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
            Status = LeadStatus.New
        });
        await db.SaveChangesAsync();
        var id = db.Leads.First().LeadId;

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.Admin);
        user.SetupGet(x => x.UserId).Returns(1);

        var handler = new UpdateLeadCommandHandler(
            db,
            user.Object,
            Mock.Of<IAnalyticsCacheInvalidator>(),
            NullLogger<UpdateLeadCommandHandler>.Instance);

        var dto = await handler.Handle(
            new UpdateLeadCommand(id, "X", "x@test.com", "1", "c", "p", "Contacted", "Web", "Low", null),
            CancellationToken.None);

        Assert.Equal("Contacted", dto.Status);
    }
}
