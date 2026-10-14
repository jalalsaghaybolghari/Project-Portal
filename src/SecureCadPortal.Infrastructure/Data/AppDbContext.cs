
using Microsoft.EntityFrameworkCore;
using SecureCadPortal.Domain.Entities;

namespace SecureCadPortal.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
}
