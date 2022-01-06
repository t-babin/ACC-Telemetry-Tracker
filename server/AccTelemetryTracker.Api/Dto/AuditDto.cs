using System.Diagnostics.CodeAnalysis;

namespace AccTelemetryTracker.Api.Dto
{
    public class AuditDto
    {
        public DateTime EventDate { get; set; }
        
        [AllowNull]
        public string EventType { get; set; }
        
        [AllowNull]
        public string Username { get; set; }
        
        [AllowNull]
        public string Log { get; set; }
    }
}