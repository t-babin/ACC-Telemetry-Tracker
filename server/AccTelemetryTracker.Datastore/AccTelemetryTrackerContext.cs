using AccTelemetryTracker.Datastore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccTelemetryTracker.Datastore;

public class AccTelemetryTrackerContext : DbContext
{
    public AccTelemetryTrackerContext(DbContextOptions<AccTelemetryTrackerContext> options)
        : base(options)
    { }

    public DbSet<Car> Cars { get; set; }

    public DbSet<Track> Tracks { get; set; }

    public DbSet<MotecFile> MotecFiles { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<AverageLap> AverageLaps { get; set; }

    public DbSet<Audit> AuditLog { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Car>()
            .ToTable("Cars");

        builder.Entity<Car>()
            .HasKey(c => c.Id);

        builder.Entity<Track>()
            .ToTable("Tracks");

        builder.Entity<Track>()
            .HasKey(t => t.Id);

        builder.Entity<MotecFile>()
            .ToTable("MotecFiles");

        builder.Entity<MotecFile>()
            .HasKey(m => m.Id);

        builder.Entity<MotecFile>()
            .HasOne(m => m.Car)
            .WithMany(c => c.MotecFiles)
            .HasForeignKey(m => m.CarId);

        builder.Entity<MotecFile>()
            .HasOne(m => m.Track)
            .WithMany(t => t.MotecFiles)
            .HasForeignKey(m => m.TrackId);

        builder.Entity<MotecFile>()
            .HasOne(m => m.User)
            .WithMany(u => u.MotecFiles)
            .HasForeignKey(m => m.UserId);

        builder.Entity<User>()
            .ToTable("Users");
        
        builder.Entity<User>()
            .HasKey(u => u.Id);

        builder.Entity<AverageLap>()
            .ToTable("AverageLaps");
        
        builder.Entity<AverageLap>()
            .HasKey(a => new { a.CarId, a.TrackId });
        
        builder.Entity<AverageLap>()
            .HasOne(a => a.Car)
            .WithMany(c => c.AverageLaps)
            .HasForeignKey(a => a.CarId);
        
        builder.Entity<AverageLap>()
            .HasOne(a => a.Track)
            .WithMany(t => t.AverageLaps)
            .HasForeignKey(a => a.TrackId);

        builder.Entity<Audit>()
            .ToTable("AuditLogs");
        
        builder.Entity<Audit>()
            .HasKey(a => a.Id);
        
        builder.Entity<Audit>()
            .HasOne(a => a.User)
            .WithMany(u => u.AuditEvents)
            .HasForeignKey(a => a.UserId);

        builder.Entity<Audit>()
            .HasOne(a => a.MotecFile)
            .WithMany(m => m.AuditEvents)
            .HasForeignKey(a => a.MotecId);
    }
}

public class AccTelemetryTrackerContextFactory : IDesignTimeDbContextFactory<AccTelemetryTrackerContext>
{
    public AccTelemetryTrackerContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AccTelemetryTrackerContext>();
        // optionsBuilder.UseSqlite("Data Source=C:\\Code\\ACC-Telemetry-Tracker\\acc.db");
        var connectionString = "server=192.168.2.13;port=3309;database=acc_telemetry_tracker;user=root;password=my-secret-pw;";
        var version = ServerVersion.AutoDetect(connectionString);
        optionsBuilder.UseMySql(connectionString, version);

        return new AccTelemetryTrackerContext(optionsBuilder.Options);
    }
}