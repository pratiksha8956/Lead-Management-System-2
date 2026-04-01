using LeadManagement.Application.Abstractions;
using LeadManagement.Application.Features.Leads.Commands.CreateLead;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
using LeadManagement.Infrastructure.Persistence;
using LeadManagement.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LeadManagement.Tests.Application;

public class CreateLeadCommandHandlerTests
{
    [Fact]
    public async Task Creates_Lead_And_Invalidates_Cache()
    {
        await using var db = TestDbContextFactory.Create();
        db.SalesReps.Add(new SalesRep { Name = "Rep", Email = "r@test.com" });
        await db.SaveChangesAsync();
        var repId = db.SalesReps.First().SalesRepId;

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.UserId).Returns(1);
        user.SetupGet(x => x.Role).Returns(UserRole.Admin);
        user.SetupGet(x => x.SalesRepId).Returns((int?)null);

        var cache = new Mock<IAnalyticsCacheInvalidator>();
        cache.Setup(x => x.InvalidateAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new CreateLeadCommandHandler(
            db,
            user.Object,
            cache.Object,
            NullLogger<CreateLeadCommandHandler>.Instance);

        var dto = await handler.Handle(
            new CreateLeadCommand("A", "a@b.com", "1", "Co", "Pos", "Web", "High", repId),
            CancellationToken.None);

        Assert.True(dto.LeadId > 0);
        Assert.True(await db.Leads.AnyAsync(l => l.Email == "a@b.com"));
        cache.Verify(x => x.InvalidateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Duplicate_Email_Throws()
    {
        await using var db = TestDbContextFactory.Create();
        db.Leads.Add(new Lead
        {
            Name = "X",
            Email = "dup@test.com",
            Phone = "1",
            Company = "c",
            Position = "p",
            Source = "Web",
            Priority = "Low",
            Status = LeadStatus.New
        });
        await db.SaveChangesAsync();

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(x => x.Role).Returns(UserRole.Admin);

        var handler = new CreateLeadCommandHandler(
            db,
            user.Object,
            Mock.Of<IAnalyticsCacheInvalidator>(),
            NullLogger<CreateLeadCommandHandler>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new CreateLeadCommand("B", "dup@test.com", "1", "Co", "Pos", "Web", "High", null),
            CancellationToken.None));
    }
}
