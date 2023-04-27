using System.Diagnostics.CodeAnalysis;

namespace AccTelemetryTracker.Api.Dto
{
    public class AuditLogDto
    {
        public int AuditCount { get; set; }

        public IEnumerable<AuditDto>? AuditEvents { get; set; }
    }

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