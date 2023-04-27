using System.Text.Json;
using AccTelemetryTracker.Api.Attributes;
using AccTelemetryTracker.Api.Dto;
using AccTelemetryTracker.Datastore;
using AccTelemetryTracker.Datastore.Models;
using AccTelemetryTracker.Logic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccTelemetryTracker.Api.Controllers
{
    [TypeFilter(typeof(CookieAuthorizeFilter))]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly AccTelemetryTrackerContext _context;
        private readonly IMapper _mapper;

        public AdminController(ILogger<AdminController> logger, AccTelemetryTrackerContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("gameversions")]
        public async Task<ActionResult> GetGameVersions()
        {
            var userValidation = await ValidateUser();
            if (userValidation.Result != null)
            {
                return userValidation.Result;
            }

            return Ok(await _context.GameVersions.OrderByDescending(v => v.StartDate).ToListAsync());
        }

        [HttpGet("tracks")]
        public async Task<ActionResult> GetTracks()
        {
            var userValidation = await ValidateUser();
            if (userValidation.Result != null)
            {
                return userValidation.Result;
            }

            return Ok(_mapper.Map<IEnumerable<AdminTrackDto>>(await _context.Tracks.OrderBy(t => t.Name).ToListAsync()));
        }

        [HttpPost("track")]
        public async Task<ActionResult> UpdateTrack([FromBody] AdminTrackDto track)
        {
            var userValidation = await ValidateUser();
            if (userValidation.Result != null)
            {
                return userValidation.Result;
            }

            var trackFromDb = await _context.Tracks.FindAsync(track.Id);
            if (trackFromDb == null)
            {
                _logger.LogInformation($"Creating new track: [{JsonSerializer.Serialize(track)}]");
                _context.AuditLog.Add(new Audit
                {
                    EventDate = DateTime.Now,
                    EventType = EventType.NewTrack,
                    Log = JsonSerializer.Serialize(track),
                    UserId = userValidation.User!.Id
                });
                _context.Tracks.Add(new Track
                {
                    MaxLapTime = track.MaxLapTime,
                    MinLapTime = track.MinLapTime,
                    MotecName = track.MotecName,
                    Name = track.Name
                });
            }
            else
            {
                _logger.LogInformation($"Updating track. Before: {JsonSerializer.Serialize(trackFromDb)}; After: {JsonSerializer.Serialize(track)}");
                _context.AuditLog.Add(new Audit
                {
                    EventDate = DateTime.Now,
                    EventType = EventType.UpdateTrack,
                    Log = $"Before: {JsonSerializer.Serialize(trackFromDb)}; After: {JsonSerializer.Serialize(track)}",
                    UserId = userValidation.User!.Id
                });
                trackFromDb.MaxLapTime = track.MaxLapTime;
                trackFromDb.MinLapTime = track.MinLapTime;
                trackFromDb.MotecName = track.MotecName;
                trackFromDb.Name = track.Name;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("cars")]
        public async Task<ActionResult> GetCars()
        {
            var userValidation = await ValidateUser();
            if (userValidation.Result != null)
            {
                return userValidation.Result;
            }

            return Ok(_mapper.Map<IEnumerable<AdminCarDto>>(await _context.Cars.OrderBy(t => t.Name).ToListAsync()));
        }

        [HttpPost("car")]
        public async Task<ActionResult> UpdateCar([FromBody] AdminCarDto car)
        {
            var userValidation = await ValidateUser();
            if (userValidation.Result != null)
            {
                return userValidation.Result;
            }

            var carFromDb = await _context.Cars.FindAsync(car.Id);
            if (carFromDb == null)
            {
                _logger.LogInformation($"Creating new car: [{JsonSerializer.Serialize(car)}]");
                _context.AuditLog.Add(new Audit
                {
                    EventDate = DateTime.Now,
                    EventType = EventType.NewCar,
                    Log = JsonSerializer.Serialize(car),
                    UserId = userValidation.User!.Id
                });
                _context.Cars.Add(new Car
                {
                    Class = (CarClass) Enum.Parse(typeof(CarClass), car.Class),
                    MotecName = car.MotecName,
                    Name = car.Name
                });
            }
            else
            {
                _logger.LogInformation($"Updating track. Before: {JsonSerializer.Serialize(carFromDb)}; After: {JsonSerializer.Serialize(car)}");
                _context.AuditLog.Add(new Audit
                {
                    EventDate = DateTime.Now,
                    EventType = EventType.UpdateCar,
                    Log = $"Before: {JsonSerializer.Serialize(carFromDb)}; After: {JsonSerializer.Serialize(car)}",
                    UserId = userValidation.User!.Id
                });
                carFromDb.Class = (CarClass) Enum.Parse(typeof(CarClass), car.Class);
                carFromDb.MotecName = car.MotecName;
                carFromDb.Name = car.Name;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("gameversion")]
        public async Task<ActionResult> UpdateGameVersion([FromBody] GameVersion version)
        {
            var userValidation = await ValidateUser();
            if (userValidation.Result != null)
            {
                return userValidation.Result;
            }

            var previousVersion = await _context.GameVersions.FindAsync(version.Id);
            if (previousVersion == null)
            {
                _logger.LogInformation($"Creating new game version: [{JsonSerializer.Serialize(version)}]");
                _context.AuditLog.Add(new Audit
                {
                    EventDate = DateTime.Now,
                    EventType = EventType.NewGameVersion,
                    Log = JsonSerializer.Serialize(version),
                    UserId = userValidation.User!.Id
                });
                _context.GameVersions.Add(new GameVersion
                {
                    StartDate = version.StartDate.Date,
                    EndDate = version.EndDate.HasValue ? version.EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : null,
                    VersionNumber = version.VersionNumber
                });
            }
            else
            {
                _logger.LogInformation($"Updating game version. Before: {JsonSerializer.Serialize(previousVersion)}; After: {JsonSerializer.Serialize(version)}");
                _context.AuditLog.Add(new Audit
                {
                    EventDate = DateTime.Now,
                    EventType = EventType.UpdateGameVersion,
                    Log = $"Before: {JsonSerializer.Serialize(previousVersion)}; After: {JsonSerializer.Serialize(version)}",
                    UserId = userValidation.User!.Id
                });
                previousVersion.StartDate = version.StartDate;
                previousVersion.EndDate = version.EndDate;
                previousVersion.VersionNumber = version.VersionNumber;
            }

            await _context.SaveChangesAsync();

            // var withoutVersion = await _context.MotecFiles.ToListAsync();
            // _logger.LogInformation($"Updating game version in [{withoutVersion.Count}] files");
            // await _parserLogic.GetGameVersion(withoutVersion);
            // await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("gameversion/{id}")]
        public async Task<ActionResult> DeleteGameVersion(int id)
        {
            _logger.LogInformation($"Deleting game version [{id}]");
            var userValidation = await ValidateUser();
            if (userValidation.Result != null)
            {
                return userValidation.Result;
            }

            var previousVersion = await _context.GameVersions.FindAsync(id);
            if (previousVersion == null)
            {
                _logger.LogWarning($"Game version not found");
                return NotFound();
            }
            _logger.LogInformation($"Deleting game version [{JsonSerializer.Serialize(previousVersion)}]");
            _context.AuditLog.Add(new Audit
            {
                EventDate = DateTime.Now,
                EventType = EventType.DeleteGameVersion,
                Log = $"Deleted: {JsonSerializer.Serialize(previousVersion)}",
                UserId = userValidation.User!.Id
            });

            _context.GameVersions.Remove(previousVersion);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("track/{id}")]
        public async Task<ActionResult> DeleteTrack(int id)
        {
            _logger.LogInformation($"Deleting Track [{id}]");
            var userValidation = await ValidateUser();
            if (userValidation.Result != null)
            {
                return userValidation.Result;
            }

            var trackFromDb = await _context.Tracks.FindAsync(id);
            if (trackFromDb == null)
            {
                _logger.LogWarning($"Track not found");
                return NotFound();
            }
            _logger.LogInformation($"Deleting Track [{JsonSerializer.Serialize(trackFromDb)}]");
            _context.AuditLog.Add(new Audit
            {
                EventDate = DateTime.Now,
                EventType = EventType.DeleteTrack,
                Log = $"Deleted: {JsonSerializer.Serialize(trackFromDb)}",
                UserId = userValidation.User!.Id
            });

            _context.Tracks.Remove(trackFromDb);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("car/{id}")]
        public async Task<ActionResult> DeleteCar(int id)
        {
            _logger.LogInformation($"Deleting Car [{id}]");
            var userValidation = await ValidateUser();
            if (userValidation.Result != null)
            {
                return userValidation.Result;
            }

            var carFromDb = await _context.Cars.FindAsync(id);
            if (carFromDb == null)
            {
                _logger.LogWarning($"Car not found");
                return NotFound();
            }
            _logger.LogInformation($"Deleting Car [{JsonSerializer.Serialize(carFromDb)}]");
            _context.AuditLog.Add(new Audit
            {
                EventDate = DateTime.Now,
                EventType = EventType.DeleteGameVersion,
                Log = $"Deleted: {JsonSerializer.Serialize(carFromDb)}",
                UserId = userValidation.User!.Id
            });

            _context.Cars.Remove(carFromDb);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<(ActionResult? Result, User? User, KeyValuePair<string, string>? Cookie)> ValidateUser()
        {
            var userCookie = HttpContext.Request.Cookies.FirstOrDefault(c => c.Key.Equals("user"));
            if (!long.TryParse(JsonDocument.Parse(userCookie.Value).RootElement.GetProperty("Id").GetString(), out var userId))
            {
                _logger.LogError("Error parsing the user cookie");
                return (Unauthorized(), null, null);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogError($"User with ID [{userId.ToString()}] doesn't exist");
                return (Unauthorized(), null, null);
            }

            if (!user.Role.Equals("admin"))
            {
                _logger.LogError($"User with ID [{userId.ToString()}] is not an admin");
                return (Unauthorized(), null, null);
            }

            return (null, user, userCookie);
        }
    }
}