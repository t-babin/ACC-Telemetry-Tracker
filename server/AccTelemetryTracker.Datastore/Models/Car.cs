namespace AccTelemetryTracker.Datastore.Models;

public class Car
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;

    public string MotecName { get; set; } = string.Empty;
    
    public CarClass Class { get; set; }

    public List<MotecFile> MotecFiles { get; set; }

    public IEnumerable<AverageLap> AverageLaps { get; set; }
}

public enum CarClass
{
    GT3,
    GT4,
    Cup,
    ST,
    CHL,
    TCX
}