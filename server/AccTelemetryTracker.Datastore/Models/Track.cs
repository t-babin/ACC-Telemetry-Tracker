namespace AccTelemetryTracker.Datastore.Models;

public class Track
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string MotecName { get; set; } = string.Empty;

    public int MinLapTime { get; set; }

    public int MaxLapTime { get; set; }

    public List<MotecFile> MotecFiles { get; set; }

    public IEnumerable<AverageLap> AverageLaps { get; set; }
}