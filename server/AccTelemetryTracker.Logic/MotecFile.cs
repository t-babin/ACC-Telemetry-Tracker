namespace AccTelemetryTracker.Logic;
public class MotecFile
{
    public string TrackName { get; set; } = string.Empty;
    
    public string MotecTrack { get; set; } = string.Empty;

    public string MotecCarName { get; set; } = string.Empty;

    public string CarName { get; set; } = string.Empty;

    public string CarClass { get; set; } = string.Empty;
    
    public DateTime Date { get; set; }

    public string GameVersion { get; set; } = string.Empty;
    
    public IEnumerable<MotecLap> Laps { get; set; } = new List<MotecLap>();
}

public class MotecLap
{
    /// <summary>
    /// The lap number as it appears in the file
    /// </summary>
    /// <value></value>
    public int LapNumber { get; set; }

    /// <summary>
    /// The lap time in seconds
    /// </summary>
    /// <value></value>
    public double LapTime { get; set; }

    /// <summary>
    /// The total session time as of this lap
    /// </summary>
    /// <value></value>
    public double SessionTime { get; set; }
}