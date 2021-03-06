namespace AccTelemetryTracker.Datastore.Models;

public class MotecFile
{
    public int Id { get; set; }
    
    public int CarId { get; set; }

    public Car Car { get; set; }
    
    public int TrackId { get; set; }

    public Track Track { get; set; }

    public long UserId { get; set; }
    
    public User User { get; set; }
    
    public DateTime DateInserted { get; set; }

    public DateTime SessionDate { get; set; }
    
    public int NumberOfLaps { get; set; }
    
    public double FastestLap { get; set; }

    public string Comment { get; set; }
    
    public string FileLocation { get; set; } = string.Empty;

    public string GameVersion { get; set; }

    public TrackCondition? TrackCondition { get; set; }
    
    public List<Audit> AuditEvents { get; set; }
}

public enum TrackCondition
{
    Green,
    Fast,
    Optimum,
    Greasy,
    Damp,
    Wet,
    Flooded
}