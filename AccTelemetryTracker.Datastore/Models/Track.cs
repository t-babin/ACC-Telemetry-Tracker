namespace AccTelemetryTracker.Datastore.Models;

public class Track
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<MotecFile> MotecFiles { get; set; }
}