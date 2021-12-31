using System.Diagnostics.CodeAnalysis;

namespace AccTelemetryTracker.Api.Dto;

public class CarDto
{
    public int Id { get; set; }

    [AllowNull]
    public string Name { get; set; }

    [AllowNull]
    public string CarClass { get; set; }
}