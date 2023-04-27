using System.Diagnostics.CodeAnalysis;

namespace AccTelemetryTracker.Api.Dto;

public class TrackDto
{
    public int Id { get; set; }

    [AllowNull]
    public string Name { get; set; }
}

public class AdminTrackDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string MotecName { get; set; } = string.Empty;

    public int MinLapTime { get; set; }

    public int MaxLapTime { get; set; }
}