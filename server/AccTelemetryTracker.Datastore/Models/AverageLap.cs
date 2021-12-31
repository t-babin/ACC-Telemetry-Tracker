namespace AccTelemetryTracker.Datastore.Models;

public class AverageLap
{
    public int CarId { get; set; }
    
    public Car Car { get; set; }
    
    public int TrackId { get; set; }
    
    public Track Track { get; set; }
    
    public double AverageFastestLap { get; set; }
}