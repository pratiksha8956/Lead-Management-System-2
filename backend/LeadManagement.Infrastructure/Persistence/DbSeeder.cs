using LeadManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, PasswordHasher<AppUser> hasher, ILogger logger, CancellationToken cancellationToken = default)
    {
        _ = hasher;
        var hasAnyUsers = await db.Users.AnyAsync(cancellationToken);
        var hasAnyLeads = await db.Leads.AnyAsync(cancellationToken);
        if (hasAnyUsers || hasAnyLeads)
        {
            logger.LogInformation("Seed skipped: existing data found.");
            return;
        }

        logger.LogInformation("No demo seed data inserted. System is ready for real data.");
    }
}
