using System.Diagnostics.CodeAnalysis;

namespace AccTelemetryTracker.Api.Dto;

public class MotecFileDto
{
    public int Id { get; set; }

    [AllowNull]
    public string CarName { get; set; }

    [AllowNull]
    public string CarClass { get; set; }
    
    [AllowNull]
    public string TrackName { get; set; }
    
    public DateTime DateInserted { get; set; }
    
    public int NumberOfLaps { get; set; }
    
    public double FastestLap { get; set; }   
}