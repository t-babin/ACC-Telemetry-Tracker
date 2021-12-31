using System.Text.Json;
using AccTelemetryTracker.Api.Attributes;
using AccTelemetryTracker.Api.Dto;
using AccTelemetryTracker.Datastore;
using AccTelemetryTracker.Datastore.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccTelemetryTracker.Api.Controllers;

[TypeFilter(typeof(CookieAuthorizeFilter))]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly AccTelemetryTrackerContext _context;
    private readonly IMapper _mapper;
    private readonly IAuditRepository _auditRepo;

    public UserController(ILogger<UserController> logger, AccTelemetryTrackerContext context, IMapper mapper, IAuditRepository auditRepo)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _auditRepo = auditRepo;
    }

    /// <summary>
    /// Gets all users from the database
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetAllUsers")]
    public async Task<ActionResult> GetAllUsers()
    {
        return Ok(_mapper.Map<IEnumerable<UserDto>>(await _context.Users.Include(u => u.MotecFiles).ToListAsync()));
    }

    /// <summary>
    /// Updates the provided collection of users
    /// </summary>
    /// <param name="users">The collection of users being updated</param>
    /// <returns></returns>
    [HttpPut(Name = "UpdateUsers")]
    public async Task<ActionResult> UpdateUsers([FromBody] UserCollectionDto users)
    {
        var userCookie = HttpContext.Request.Cookies.FirstOrDefault(c => c.Key.Equals("user"));
        if (!long.TryParse(JsonDocument.Parse(userCookie.Value).RootElement.GetProperty("Id").GetString(), out var userId))
        {
            _logger.LogError("Error parsing the user cookie");
            return Unauthorized();
        }

        var userFromDb = await _context.Users.FindAsync(userId);
        if (userFromDb == null)
        {
            _logger.LogError($"User with ID [{userId.ToString()}] doesn't exist");
            return Unauthorized();
        }
        if (!userFromDb.Role.Equals("admin"))
        {
            _logger.LogError($"User with ID [{userId.ToString()}] is not an admin");
            return Unauthorized();
        }
        
        foreach (var user in users.Users)
        {
            var fromDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == long.Parse(user.Id));
            if (fromDb is null)
            {
                _logger.LogError($"The user with ID [{user.Id}] cannot be found in the database");
                return NotFound();
            }

            if (!fromDb.Role.Equals(user.Role))
            {
                fromDb.Role = user.Role;
            }

            if (fromDb.IsValid != user.IsValid)
            {
                fromDb.IsValid = user.IsValid;
            }
        }

        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync();

            await _auditRepo.PostAuditEvent(EventType.DownloadFile, userId, $"Updated users: [{string.Join(",", users.Users.Select(u => u.Id))}]");
        }

        return Ok();
    }
}