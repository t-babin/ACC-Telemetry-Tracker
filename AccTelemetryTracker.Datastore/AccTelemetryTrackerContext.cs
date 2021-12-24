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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Car>().ToTable("Cars");

        builder.Entity<Car>()
            .HasKey(c => c.Id);

        builder.Entity<Track>().ToTable("Tracks");

        builder.Entity<Track>()
            .HasKey(t => t.Id);

        builder.Entity<MotecFile>().ToTable("MotecFiles");

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
    }
}

public class AccTelemetryTrackerContextFactory : IDesignTimeDbContextFactory<AccTelemetryTrackerContext>
{
    public AccTelemetryTrackerContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AccTelemetryTrackerContext>();
        optionsBuilder.UseSqlite("Data Source=C:\\Code\\ACC-Telemetry-Tracker\\acc.db");

        return new AccTelemetryTrackerContext(optionsBuilder.Options);
    }
}