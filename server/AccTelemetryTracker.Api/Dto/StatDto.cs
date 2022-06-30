using System.Diagnostics.CodeAnalysis;

namespace AccTelemetryTracker.Api.Dto;
public class MotecStatDto
{
    [AllowNull]
    public string Track { get; set; }

    public int TrackId { get; set; }
    
    [AllowNull]
    public string Car { get; set; }

    public int CarId { get; set; }
    
    [AllowNull]
    public string TrackCondition { get; set; }
    
    public double FastestLap { get; set; }
    
    public double AverageFastestLap { get; set; }
}