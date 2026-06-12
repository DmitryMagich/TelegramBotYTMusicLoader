using Microsoft.EntityFrameworkCore;
using TelegramBotYtMusic.Entities;

namespace TelegramBotYtMusic.Database;

public class ServiceDbContext(DbContextOptions<ServiceDbContext> options) : DbContext(options)
{
    public DbSet<TrackReference> TrackCaches => Set<TrackReference>();
    public DbSet<AppUser> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TrackReference>()
            .HasIndex(t => t.SearchQuery);
    }
}