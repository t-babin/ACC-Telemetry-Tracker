namespace AccTelemetryTracker.Datastore.Models
{
    public class User
    {
        public long Id { get; set; }

        public string Username { get; set; }
        
        public string ServerName { get; set; }
        
        public bool IsValid { get; set; }

        public string Role { get; set; }

        public DateTime SignupDate { get; set; }

        public List<MotecFile> MotecFiles { get; set; }

        public List<Audit> AuditEvents { get; set; }
    }
}