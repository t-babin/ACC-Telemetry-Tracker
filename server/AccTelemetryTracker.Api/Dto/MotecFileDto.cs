using System.Diagnostics.CodeAnalysis;
using AccTelemetryTracker.Logic;

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

    [AllowNull]
    public string Username { get; set; }
    
    public DateTime DateInserted { get; set; }

    public DateTime SessionDate { get; set; }
    
    public int NumberOfLaps { get; set; }
    
    [AllowNull]
    public string Comment { get; set; }
    
    public double FastestLap { get; set; }

    public IEnumerable<MotecLap> Laps { get; set; } = new List<MotecLap>();
}

public class MotecFileCommentDto
{
    public int Id { get; set; }

    [AllowNull]
    public string Comment { get; set; }
}