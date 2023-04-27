namespace AccTelemetryTracker.Datastore.Models
{
    public class GameVersion
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string VersionNumber { get; set; }
    }
}