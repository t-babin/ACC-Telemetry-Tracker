using System.Diagnostics.CodeAnalysis;

namespace AccTelemetryTracker.Api.Dto
{
    public class AverageLapDto
    {
        public int CarId { get; set; }
        
        [AllowNull]
        public string Car { get; set; }
        
        public int TrackId { get; set; }

        [AllowNull]
        public string TrackName { get; set; }
        
        public double FastestLap { get; set; }
    }
}