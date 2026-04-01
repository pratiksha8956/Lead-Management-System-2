using LeadManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeadManagement.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Lead> Leads { get; }
    DbSet<Interaction> Interactions { get; }
    DbSet<AppUser> Users { get; }
    DbSet<SalesRep> SalesReps { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
