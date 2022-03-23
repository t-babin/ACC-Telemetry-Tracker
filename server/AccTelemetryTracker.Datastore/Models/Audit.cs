namespace AccTelemetryTracker.Datastore.Models;
public class Audit
{
    public int Id { get; set; }
    
    public DateTime EventDate { get; set; }
    
    public EventType EventType { get; set; }
    
    public long UserId { get; set; }
    
    public User User { get; set; }
    
    public int? MotecId { get; set; }
    
    public MotecFile MotecFile { get; set; }

    public string Log { get; set; }   
}

public enum EventType
{
    Authenticate,
    UploadFile,
    DownloadFile,
    ModifyUser,
    GetLaps,
    UpdateComment,
    UpdateTrackCondition
}