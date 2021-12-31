using System.Diagnostics.CodeAnalysis;

namespace AccTelemetryTracker.Api.Dto;

public class TrackDto
{
    public int Id { get; set; }

    [AllowNull]
    public string Name { get; set; }
}