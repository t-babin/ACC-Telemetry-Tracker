namespace AccTelemetryTracker.Datastore.Models;

public class Car
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Class { get; set; } = string.Empty;

    public List<MotecFile> MotecFiles { get; set; }

    public IEnumerable<AverageLap> AverageLaps { get; set; }
}
