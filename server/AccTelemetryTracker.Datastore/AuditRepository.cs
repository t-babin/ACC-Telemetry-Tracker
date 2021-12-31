using AccTelemetryTracker.Datastore.Models;
using Microsoft.Extensions.Logging;

namespace AccTelemetryTracker.Datastore;

public class AuditRepository : IAuditRepository
{
    private readonly ILogger<AuditRepository> _logger;
    private readonly AccTelemetryTrackerContext _context;

    public AuditRepository(ILogger<AuditRepository> logger, AccTelemetryTrackerContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    public async Task PostAuditEvent(EventType eventType, long userId, string log, int? motecId = null)
    {
        _logger.LogInformation($"[{userId}] performed [{eventType}]. Log: [{log}]; MotecId [{(motecId.HasValue ? motecId.Value : -1)}]");
        _context.AuditLog.Add(new Audit
        {
            EventDate = DateTime.Now,
            EventType = eventType,
            Log = log,
            MotecId = motecId,
            UserId = userId
        });

        await _context.SaveChangesAsync();
    }
}
