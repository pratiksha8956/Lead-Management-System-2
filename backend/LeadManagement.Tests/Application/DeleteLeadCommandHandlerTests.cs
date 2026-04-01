using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Features.Leads.Commands.DeleteLead;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
using LeadManagement.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LeadManagement.Tests.Application;

public class DeleteLeadCommandHandlerTests
{
    [Fact]
    public async Task Cannot_Delete_Converted()
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
            Status = LeadStatus.Converted
        });
        await db.SaveChangesAsync();
        var id = db.Leads.First().LeadId;

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.Admin);
        user.SetupGet(x => x.UserId).Returns(1);

        var handler = new DeleteLeadCommandHandler(
            db,
            user.Object,
            Mock.Of<IAnalyticsCacheInvalidator>(),
            NullLogger<DeleteLeadCommandHandler>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new DeleteLeadCommand(id), CancellationToken.None));
    }

    [Fact]
    public async Task Deletes_When_Allowed()
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

        var handler = new DeleteLeadCommandHandler(
            db,
            user.Object,
            Mock.Of<IAnalyticsCacheInvalidator>(),
            NullLogger<DeleteLeadCommandHandler>.Instance);

        await handler.Handle(new DeleteLeadCommand(id), CancellationToken.None);
        Assert.False(await db.Leads.AnyAsync());
    }
}
