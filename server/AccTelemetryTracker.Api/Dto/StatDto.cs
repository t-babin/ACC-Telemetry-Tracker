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

public class UserMetricDto
{
    public string? User { get; set; }

    public long UserId { get; set; }

    public string? FavouriteCar { get; set; }

    public string? FavouriteTrack { get; set; }

    public int NumberOfFastestLaps { get; set; }

    public int NumberOfLaps { get; set; }

    public int NumberOfUploads { get; set; }
}