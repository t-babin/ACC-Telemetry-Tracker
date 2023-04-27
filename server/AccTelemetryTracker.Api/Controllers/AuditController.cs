using AccTelemetryTracker.Api.Attributes;
using AccTelemetryTracker.Api.Dto;
using AccTelemetryTracker.Datastore;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccTelemetryTracker.Api.Controllers
{
    [TypeFilter(typeof(CookieAuthorizeFilter))]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly ILogger<AuditController> _logger;
        private readonly AccTelemetryTrackerContext _context;
        private readonly IMapper _mapper;

        public AuditController(ILogger<AuditController> logger, AccTelemetryTrackerContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet(Name = "AllAuditLogs")]
        public async Task<ActionResult> GetAuditLogs([FromQuery] int? take, [FromQuery] int? skip)
        {
            return Ok(new AuditLogDto
            {
                AuditEvents = _mapper.Map<IEnumerable<AuditDto>>(await _context.AuditLog
                    .AsNoTracking()
                    .Include(a => a.User)
                    .OrderByDescending(a => a.EventDate)
                    .Skip(skip.HasValue ? skip.Value : 0)
                    .Take(take.HasValue ? take.Value : 10)
                    .ToListAsync()),
                AuditCount = await _context.AuditLog.AsNoTracking().CountAsync()
            });
        }
    }
}