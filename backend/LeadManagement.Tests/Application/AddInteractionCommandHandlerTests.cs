using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Features.Interactions.Commands.AddInteraction;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
using LeadManagement.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LeadManagement.Tests.Application;

public class AddInteractionCommandHandlerTests
{
    [Fact]
    public async Task Future_Interaction_Date_Throws()
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
            Status = LeadStatus.Contacted
        });
        await db.SaveChangesAsync();
        var id = db.Leads.First().LeadId;

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.Admin);

        var handler = new AddInteractionCommandHandler(
            db,
            user.Object,
            NullLogger<AddInteractionCommandHandler>.Instance);

        var future = DateTime.UtcNow.AddDays(2);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new AddInteractionCommand(id, "n", future, null), CancellationToken.None));
    }

    [Fact]
    public async Task FollowUp_Must_Be_After_Interaction()
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
            Status = LeadStatus.Contacted
        });
        await db.SaveChangesAsync();
        var id = db.Leads.First().LeadId;

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.Admin);

        var handler = new AddInteractionCommandHandler(
            db,
            user.Object,
            NullLogger<AddInteractionCommandHandler>.Instance);

        var d = DateTime.UtcNow.AddDays(-1);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new AddInteractionCommand(id, "n", d, d.AddHours(-1)), CancellationToken.None));
    }

    [Fact]
    public async Task No_Interaction_On_Converted()
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

        var handler = new AddInteractionCommandHandler(
            db,
            user.Object,
            NullLogger<AddInteractionCommandHandler>.Instance);

        var d = DateTime.UtcNow.AddDays(-1);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new AddInteractionCommand(id, "n", d, null), CancellationToken.None));
    }
}
