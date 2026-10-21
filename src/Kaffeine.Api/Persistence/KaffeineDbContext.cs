using Microsoft.EntityFrameworkCore;

namespace Kaffeine.Api.Persistence;

/// <summary>
/// Scoped unit of work for application state. Features own their entities and mappings.
/// </summary>
public sealed class KaffeineDbContext(DbContextOptions<KaffeineDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KaffeineDbContext).Assembly);
    }
}
