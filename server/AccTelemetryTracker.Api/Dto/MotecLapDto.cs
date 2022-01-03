using AccTelemetryTracker.Logic;

namespace AccTelemetryTracker.Api.Dto;

public class MotecLapDto
{
    public double CarTrackAverageLap { get; set; }
    
    public double ClassAverageLap { get; set; }
    
    public double ClassBestLap { get; set; }
    
    public IEnumerable<MotecLap> Laps { get; set; } = new List<MotecLap>();
}
