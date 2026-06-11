using Microsoft.EntityFrameworkCore;
using TelegramBotYtMusic.Entities;

namespace TelegramBotYtMusic.Data;

public class ServiceDbContext(DbContextOptions<ServiceDbContext> options) : DbContext(options)
{
    public DbSet<TrackReference> TrackCaches => Set<TrackReference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TrackReference>()
            .HasIndex(t => t.SearchQuery);
    }
}