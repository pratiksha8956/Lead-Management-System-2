using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadManagement.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, PasswordHasher<AppUser> hasher, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await db.SalesReps.AnyAsync(cancellationToken))
            return;

        logger.LogInformation("Seeding database...");

        var rep1 = new SalesRep { Name = "Aman Patel", Email = "aman@demo.com" };
        var rep2 = new SalesRep { Name = "Riya Mehta", Email = "riya@demo.com" };
        db.SalesReps.AddRange(rep1, rep2);
        await db.SaveChangesAsync(cancellationToken);

        var users = new[]
        {
            new AppUser
            {
                Email = "admin@demo.com",
                DisplayName = "Admin User",
                Role = UserRole.Admin,
                SalesRepId = null,
                PasswordHash = string.Empty
            },
            new AppUser
            {
                Email = "manager@demo.com",
                DisplayName = "Manager User",
                Role = UserRole.SalesManager,
                SalesRepId = null,
                PasswordHash = string.Empty
            },
            new AppUser
            {
                Email = "sales@demo.com",
                DisplayName = "Sales User",
                Role = UserRole.SalesRep,
                SalesRepId = rep1.SalesRepId,
                PasswordHash = string.Empty
            }
        };

        foreach (var u in users)
            u.PasswordHash = hasher.HashPassword(u, MapPassword(u.Email));

        db.Users.AddRange(users);
        await db.SaveChangesAsync(cancellationToken);

        var sampleLeads = new[]
        {
            new Lead { Name = "John Carter", Email = "john.carter@example.com", Phone = "+1-202-555-0171", Company = "Acme Corp", Position = "CTO", Status = LeadStatus.New, Source = "Website", Priority = "High", AssignedSalesRepId = rep1.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-3), ModifiedDate = DateTime.UtcNow.AddDays(-1) },
            new Lead { Name = "Neha Sharma", Email = "neha.sharma@example.com", Phone = "+91-9876543210", Company = "Nova Solutions", Position = "Product Manager", Status = LeadStatus.Contacted, Source = "Referral", Priority = "Medium", AssignedSalesRepId = rep2.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-5), ModifiedDate = DateTime.UtcNow.AddDays(-2) },
            new Lead { Name = "Michael Brown", Email = "michael.brown@example.com", Phone = "+1-303-555-0114", Company = "BlueOrbit", Position = "Head of Sales", Status = LeadStatus.Qualified, Source = "Event", Priority = "Low", AssignedSalesRepId = rep1.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-9), ModifiedDate = DateTime.UtcNow.AddDays(-1) },
            new Lead { Name = "Priya Desai", Email = "priya.desai@example.com", Phone = "+91-9810011223", Company = "TechNova", Position = "Engineering Manager", Status = LeadStatus.New, Source = "Website", Priority = "Medium", AssignedSalesRepId = rep2.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-2), ModifiedDate = DateTime.UtcNow.AddDays(-2) },
            new Lead { Name = "Arjun Verma", Email = "arjun.verma@example.com", Phone = "+91-9810011224", Company = "FinEdge", Position = "VP Finance", Status = LeadStatus.Contacted, Source = "ColdCall", Priority = "High", AssignedSalesRepId = rep1.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-8), ModifiedDate = DateTime.UtcNow.AddDays(-4) },
            new Lead { Name = "Sara Khan", Email = "sara.khan@example.com", Phone = "+1-202-555-0111", Company = "Orbit Labs", Position = "Operations Director", Status = LeadStatus.Qualified, Source = "Referral", Priority = "High", AssignedSalesRepId = rep2.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-12), ModifiedDate = DateTime.UtcNow.AddDays(-3) },
            new Lead { Name = "David Lee", Email = "david.lee@example.com", Phone = "+1-202-555-0112", Company = "GreenLeaf", Position = "Procurement Lead", Status = LeadStatus.Unqualified, Source = "Partner", Priority = "Low", AssignedSalesRepId = rep1.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-11), ModifiedDate = DateTime.UtcNow.AddDays(-7) },
            new Lead { Name = "Ananya Iyer", Email = "ananya.iyer@example.com", Phone = "+91-9810011225", Company = "ScaleBridge", Position = "COO", Status = LeadStatus.Contacted, Source = "Event", Priority = "Medium", AssignedSalesRepId = rep2.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-6), ModifiedDate = DateTime.UtcNow.AddDays(-2) },
            new Lead { Name = "Rohan Gupta", Email = "rohan.gupta@example.com", Phone = "+91-9810011226", Company = "ByteForge", Position = "CTO", Status = LeadStatus.New, Source = "Website", Priority = "High", AssignedSalesRepId = rep1.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-1), ModifiedDate = DateTime.UtcNow.AddDays(-1) },
            new Lead { Name = "Maya Nair", Email = "maya.nair@example.com", Phone = "+91-9810011227", Company = "CloudNest", Position = "Product Owner", Status = LeadStatus.Contacted, Source = "Referral", Priority = "Medium", AssignedSalesRepId = rep2.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-10), ModifiedDate = DateTime.UtcNow.AddDays(-5) },
            new Lead { Name = "Ethan Clark", Email = "ethan.clark@example.com", Phone = "+1-202-555-0113", Company = "Northwind", Position = "Director", Status = LeadStatus.Qualified, Source = "Event", Priority = "High", AssignedSalesRepId = rep1.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-14), ModifiedDate = DateTime.UtcNow.AddDays(-3) },
            new Lead { Name = "Isha Jain", Email = "isha.jain@example.com", Phone = "+91-9810011228", Company = "ZenMob", Position = "Marketing Head", Status = LeadStatus.New, Source = "ColdCall", Priority = "Low", AssignedSalesRepId = rep2.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-4), ModifiedDate = DateTime.UtcNow.AddDays(-4) },
            new Lead { Name = "Kabir Singh", Email = "kabir.singh@example.com", Phone = "+91-9810011229", Company = "BluePeak", Position = "Lead Architect", Status = LeadStatus.Contacted, Source = "Partner", Priority = "Medium", AssignedSalesRepId = rep1.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-13), ModifiedDate = DateTime.UtcNow.AddDays(-6) },
            new Lead { Name = "Aisha Malik", Email = "aisha.malik@example.com", Phone = "+1-202-555-0114", Company = "Sunrise AI", Position = "CEO", Status = LeadStatus.Qualified, Source = "Website", Priority = "High", AssignedSalesRepId = rep2.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-15), ModifiedDate = DateTime.UtcNow.AddDays(-2) },
            new Lead { Name = "Nikhil Rao", Email = "nikhil.rao@example.com", Phone = "+91-9810011230", Company = "DataWave", Position = "Analytics Manager", Status = LeadStatus.Unqualified, Source = "Referral", Priority = "Low", AssignedSalesRepId = rep1.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-16), ModifiedDate = DateTime.UtcNow.AddDays(-8) },
            new Lead { Name = "Olivia Scott", Email = "olivia.scott@example.com", Phone = "+1-202-555-0115", Company = "PrimeCore", Position = "VP Product", Status = LeadStatus.Contacted, Source = "Event", Priority = "High", AssignedSalesRepId = rep2.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-7), ModifiedDate = DateTime.UtcNow.AddDays(-3) },
            new Lead { Name = "Vikram Patel", Email = "vikram.patel@example.com", Phone = "+91-9810011231", Company = "InnoGrid", Position = "Founder", Status = LeadStatus.New, Source = "Website", Priority = "Medium", AssignedSalesRepId = rep1.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-2), ModifiedDate = DateTime.UtcNow.AddDays(-2) },
            new Lead { Name = "Emma Watson", Email = "emma.watson@example.com", Phone = "+1-202-555-0116", Company = "ClearPath", Position = "Procurement Manager", Status = LeadStatus.Contacted, Source = "ColdCall", Priority = "Medium", AssignedSalesRepId = rep2.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-9), ModifiedDate = DateTime.UtcNow.AddDays(-4) },
            new Lead { Name = "Harsh Mehra", Email = "harsh.mehra@example.com", Phone = "+91-9810011232", Company = "StratoSys", Position = "CTO", Status = LeadStatus.Qualified, Source = "Partner", Priority = "High", AssignedSalesRepId = rep1.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-18), ModifiedDate = DateTime.UtcNow.AddDays(-3) },
            new Lead { Name = "Zara Ali", Email = "zara.ali@example.com", Phone = "+1-202-555-0117", Company = "QuickServe", Position = "Operations Manager", Status = LeadStatus.New, Source = "Referral", Priority = "Low", AssignedSalesRepId = rep2.SalesRepId, CreatedDate = DateTime.UtcNow.AddDays(-5), ModifiedDate = DateTime.UtcNow.AddDays(-5) }
        };

        db.Leads.AddRange(sampleLeads);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Database seed completed.");
    }

    private static string MapPassword(string email) => email.ToLowerInvariant() switch
    {
        "admin@demo.com" => "admin123",
        "manager@demo.com" => "manager123",
        "sales@demo.com" => "sales123",
        _ => "ChangeMe!123"
    };
}
