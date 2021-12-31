using AccTelemetryTracker.Datastore.Models;

namespace AccTelemetryTracker.Datastore;

public interface IAuditRepository
{
    Task PostAuditEvent(EventType eventType, long userId, string log, int? motecId = null);
}