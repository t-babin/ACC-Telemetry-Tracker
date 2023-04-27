using System.Diagnostics.CodeAnalysis;

namespace AccTelemetryTracker.Api.Dto;

public class CarDto
{
    public int Id { get; set; }

    [AllowNull]
    public string Name { get; set; }

    [AllowNull]
    public string Class { get; set; }
}

public class AdminCarDto
{
    public int Id { get; set; }

    [AllowNull]
    public string Name { get; set; }
    
    [AllowNull]
    public string MotecName { get; set; }

    [AllowNull]
    public string Class { get; set; }
}