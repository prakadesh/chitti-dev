using Microsoft.EntityFrameworkCore;
using Chitti.Models;
using Chitti.Helpers;

namespace Chitti.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<ClipboardHistory> ClipboardHistory { get; set; } = null!;
    public DbSet<AppSettings> AppSettings { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={AppPaths.DatabasePath}");
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ClipboardHistory>()
            .Property(h => h.OriginalText)
            .IsRequired();

        modelBuilder.Entity<ClipboardHistory>()
            .Property(h => h.ProcessedText)
            .IsRequired();

        modelBuilder.Entity<ClipboardHistory>()
            .Property(h => h.Tags)
            .IsRequired();

        modelBuilder.Entity<ClipboardHistory>()
            .Property(h => h.Status)
            .IsRequired();

        modelBuilder.Entity<AppSettings>()
            .Property(s => s.ApiKey)
            .IsRequired();
    }
} 