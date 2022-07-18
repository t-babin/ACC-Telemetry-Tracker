using AccTelemetryTracker.Api.Dto;
using AccTelemetryTracker.Datastore;
using AccTelemetryTracker.Api.Helpers;
using AccTelemetryTracker.Logic;
using AccTelemetryTracker.Datastore.Models;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;

using System.IO.Compression;
using AccTelemetryTracker.Api.Attributes;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AccTelemetryTracker.Api.Controllers;

[TypeFilter(typeof(CookieAuthorizeFilter))]
[ApiController]
[Route("api/[controller]")]
public class MotecController : ControllerBase
{
    private readonly ILogger<MotecController> _logger;
    private readonly AccTelemetryTrackerContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly IParserLogic _parser;
    private readonly IAuditRepository _auditRepo;
    private readonly IDiscordNotifier _discordNotifier;
    private readonly string _storagePage;

    public MotecController(ILogger<MotecController> logger, AccTelemetryTrackerContext context, IMapper mapper, IConfiguration config, IParserLogic parser,
        IAuditRepository auditRepo, IDiscordNotifier discordNotifier)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _config = config;
        _parser = parser;
        _auditRepo = auditRepo;
        _discordNotifier = discordNotifier;
        _storagePage = string.IsNullOrEmpty(_config.GetValue<string>("STORAGE_PATH")) ? "files" : _config.GetValue<string>("STORAGE_PATH");
    }

    /// <summary>
    /// Gets all of the motec files currently stored
    /// </summary>
    /// <param name="carIds">Optional list of car IDs to filter on</param>
    /// <param name="trackIds">Optional list of track IDs to filter on</param>
    /// <param name="userIds">Optional list of user IDs to filter on</param>
    /// <param name="take">Optional number of files to take (for pagination)</param>
    /// <param name="skip">Optional number of files to skip (for pagination)</param>
    /// <returns></returns>
    [HttpGet(Name = "GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll([FromQuery] IEnumerable<int>? carIds, [FromQuery] IEnumerable<int>? trackIds,
        [FromQuery] IEnumerable<long>? userIds, [FromQuery] int? take, [FromQuery] int? skip, [FromQuery] string? sortOn)
    {
        var userCookie = HttpContext.Request.Cookies.FirstOrDefault(c => c.Key.Equals("user"));
        if (!long.TryParse(JsonDocument.Parse(userCookie.Value).RootElement.GetProperty("Id").GetString(), out var userId))
        {
            _logger.LogError("Error parsing the user cookie");
            return Unauthorized();
        }

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        _logger.LogInformation($"User [{userId}] is requesting all Motec files");
        var result = await GetMotecFiles(carIds, trackIds, userIds);
        if (result.result is not null)
        {
            return result.result;
        }

        List<Datastore.Models.MotecFile> motecFiles;
        if (!string.IsNullOrEmpty(sortOn))
        {
            var sort = sortOn.Split("-");
            motecFiles = await result.files!.ToListAsync();
            switch (sort[0].ToLower())
            {
                case "track":
                    motecFiles = sort[1].ToLower().Equals("asc")
                        ? motecFiles.OrderBy(m => m.Track.Name)
                            .ThenBy(m => m.FastestLap)
                            .ToList()
                        : motecFiles.OrderByDescending(m => m.Track.Name)
                            .ThenBy(m => m.FastestLap)
                            .ToList();
                    break;

                case "fastestlap":
                    motecFiles = sort[1].ToLower().Equals("asc")
                        ? motecFiles.OrderBy(m => m.FastestLap).ToList()
                        : motecFiles.OrderByDescending(m => m.FastestLap).ToList();
                    break;

                case "dateloaded":
                    motecFiles = sort[1].ToLower().Equals("asc")
                        ? motecFiles.OrderBy(m => m.DateInserted)
                            .ThenBy(m => m.FastestLap)
                            .ToList()
                        : motecFiles.OrderByDescending(m => m.DateInserted)
                            .ThenBy(m => m.FastestLap)
                            .ToList();
                    break;
                default:
                    motecFiles = motecFiles.OrderBy(m => m.Track.Name)
                        .ThenBy(m => m.Car.Name)
                            .ThenBy(m => m.FastestLap)
                        .ToList();
                    break;
            }

            motecFiles = motecFiles
                .Skip(skip.HasValue ? skip.Value : 0)
                .Take(take.HasValue ? take.Value : 10)
                .ToList();
        }
        else
        {
            motecFiles = await result.files!
                .OrderBy(m => m.Track.Name)
                    .ThenBy(m => m.Car.Name)
                        .ThenBy(m => m.FastestLap)
                .Skip(skip.HasValue ? skip.Value : 0)
                .Take(take.HasValue ? take.Value : 10)
                .ToListAsync();
        }

        return Ok(_mapper.Map<IEnumerable<MotecFileDto>>(motecFiles));
    }

    /// <summary>
    /// Returns the total motec file count in the database
    /// </summary>
    /// <returns></returns>
    [HttpGet("count", Name = "GetMotecFileCount")]
    public async Task<ActionResult> GetFileCount([FromQuery] IEnumerable<int>? carIds, [FromQuery] IEnumerable<int>? trackIds,
        [FromQuery] IEnumerable<long>? userIds)
    {
        var result = await GetMotecFiles(carIds, trackIds, userIds);
        if (result.result is not null)
        {
            return result.result;
        }

        return Ok(await result.files!.AsNoTracking().CountAsync());
    }

    /// <summary>
    /// Gets all of the cars that are currently stored in the database
    /// </summary>
    /// <returns></returns>
    [HttpGet("cars", Name = "GetAllCars")]
    public async Task<ActionResult> GetAlLCars()
    {
        return Ok(_mapper.Map<IEnumerable<CarDto>>(await _context.MotecFiles.AsNoTracking().Include(m => m.Car).Select(m => m.Car).Distinct().OrderBy(c => c.Name).ToListAsync()));
    }

    /// <summary>
    /// Gets all of the tracks that are currently stored in the database
    /// </summary>
    /// <returns></returns>
    [HttpGet("tracks", Name = "GetAllTracks")]
    public async Task<ActionResult> GetAlLTracks()
    {
        return Ok(_mapper.Map<IEnumerable<TrackDto>>(await _context.MotecFiles.AsNoTracking().Include(m => m.Track).Select(m => m.Track).Distinct().OrderBy(t => t.Name).ToListAsync()));
    }

    /// <summary>
    /// Uploads a zipped motec file to the database. Required that the zip has the .ld and .ldx file in it and nothing else.
    /// Only one file is allowed to be uploaded at a time
    /// </summary>
    /// <returns></returns>
    [HttpPost(Name = "UploadFile")]
    [RequestSizeLimit(150_000_000)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult> UploadFile()
    {
        var request = HttpContext.Request;

        if (!request.HasFormContentType
            || !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader)
            || string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
        {
            _logger.LogError("Unexpected request content type");
            return new UnsupportedMediaTypeResult();
        }

        var userCookie = request.Cookies.FirstOrDefault(c => c.Key.Equals("user"));
        if (!long.TryParse(JsonDocument.Parse(userCookie.Value).RootElement.GetProperty("Id").GetString(), out var userId))
        {
            _logger.LogError("Error parsing the user cookie");
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogError($"User with ID [{userId.ToString()}] doesn't exist");
            return Unauthorized();
        }
        if (!user.IsValid)
        {
            _logger.LogError($"User [{userId.ToString()}] has not been activated");
            return Unauthorized();
        }
        _logger.LogInformation($"User [{user.Id}] is attempting to upload a file.");

        var reader = new MultipartReader(mediaTypeHeader.Boundary.Value, request.Body);
        var section = await reader.ReadNextSectionAsync();

        while (section != null)
        {
            var hasContentDisposition = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
            if (hasContentDisposition && contentDisposition!.DispositionType.Equals("form-data") && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                _logger.LogInformation($"Got file [{System.Web.HttpUtility.HtmlEncode(Path.GetFileName(contentDisposition.FileName.Value))}]");
                var fileName = Path.GetRandomFileName();
                var savePath = Path.Combine(_storagePage, $"{fileName}.zip");
                var tempDir = Directory.CreateDirectory(Path.Combine(_storagePage, $"TEMP-{fileName}"));

                try
                {
                    var validBytes = await FileHelper.ProcessStreamedFile(section, contentDisposition);
                    if (!validBytes.Any())
                    {
                        _logger.LogError("Error occurred when processing the file stream. Possible invalid extension");
                        return BadRequest(new { Message = "Error occurred processing file" });
                    }

                    using (var targetStream = System.IO.File.Create(savePath))
                    {
                        await targetStream.WriteAsync(validBytes);
                    }

                    _logger.LogInformation($"Saved the temp file [{savePath}] and created the temp archive directory [{tempDir}]");

                    ZipFile.ExtractToDirectory(savePath, tempDir.FullName);
                    var motecFile = await _parser.ParseFilesAsync(Directory.GetFiles(tempDir.FullName));

                    if (motecFile == null)
                    {
                        return BadRequest(new { Message = "Error parsing the MoTeC files" });
                    }

                    var existingMotec = await _context.MotecFiles
                        .Include(m => m.Car)
                        .Include(m => m.Track)
                        .FirstOrDefaultAsync(m => m.SessionDate.Equals(motecFile.Date) && m.Car.Name.Equals(motecFile.Car) && m.Track.Name.Equals(motecFile.Track));

                    if (existingMotec != null)
                    {
                        _logger.LogInformation($"Motec file [{existingMotec.Id}] already exists. Deleting the file [{savePath}]");
                        System.IO.File.Delete(savePath);
                        return BadRequest(new { Message = "This motec file has already been uploaded" });
                    }

                    var car = await _context.Cars.FirstOrDefaultAsync(c => c.Name.Equals(motecFile.Car));
                    var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Name.Equals(motecFile.Track));
                    if (car == null)
                    {
                        car = new Car { Name = motecFile.Car, Class = motecFile.CarClass };
                        _logger.LogInformation($"Creating new car [{car.Name}][{car.Class}]");
                        _context.Cars.Add(car);
                        await _context.SaveChangesAsync();
                    }
                    if (track == null)
                    {
                        track = new Track { Name = motecFile.Track };
                        _logger.LogInformation($"Creating new track [{track.Name}]");
                        _context.Tracks.Add(track);
                        await _context.SaveChangesAsync();
                    }

                    var dbMotecFile = new Datastore.Models.MotecFile
                    {
                        TrackId = track.Id,
                        CarId = car.Id,
                        DateInserted = DateTime.Now,
                        NumberOfLaps = motecFile.Laps.Count(),
                        FileLocation = $"{fileName}.zip",
                        FastestLap = motecFile.Laps.Min(l => l.LapTime),
                        SessionDate = motecFile.Date,
                        UserId = userId,
                        GameVersion = motecFile.GameVersion
                    };
                    _context.MotecFiles.Add(dbMotecFile);

                    await _context.SaveChangesAsync();

                    if (await _context.MotecFiles.AnyAsync(m => string.IsNullOrEmpty(m.GameVersion)))
                    {
                        var withoutVersion = await _context.MotecFiles.Where(m => string.IsNullOrEmpty(m.GameVersion)).ToListAsync();
                        _parser.GetGameVersion(withoutVersion);
                        await _context.SaveChangesAsync();
                    }

                    await _auditRepo.PostAuditEvent(EventType.UploadFile, userId, $"Uploaded file [{dbMotecFile.Id}][{dbMotecFile.FileLocation}]", dbMotecFile.Id);

                    return Ok(_mapper.Map<MotecFileDto>(dbMotecFile));
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, ex.Message);
                    return BadRequest(new { Message = $"Error uploading file [{System.Web.HttpUtility.HtmlEncode(Path.GetFileName(contentDisposition.FileName.Value))}]: {ex.Message}" });
                }
                catch (MotecParseException ex)
                {
                    _logger.LogError(ex, ex.Message);
                    _logger.LogInformation($"Deleting the uploaded file [{savePath}]");
                    System.IO.File.Delete(savePath);
                    return BadRequest(new { Message = $"Error uploading file [{System.Web.HttpUtility.HtmlEncode(Path.GetFileName(contentDisposition.FileName.Value))}]: {ex.Message}" });
                }
                catch (KeyNotFoundException ex)
                {
                    _logger.LogError(ex, ex.Message);
                    _logger.LogInformation($"Deleting the uploaded file [{savePath}]");
                    System.IO.File.Delete(savePath);
                    return BadRequest(new { Message = $"Error uploading file [{System.Web.HttpUtility.HtmlEncode(Path.GetFileName(contentDisposition.FileName.Value))}]: {ex.Message}" });
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown exception occurred");
                    _logger.LogError(ex, ex.Message);
                    _logger.LogInformation($"Deleting the uploaded file [{savePath}]");
                    System.IO.File.Delete(savePath);
                    return BadRequest(new { Message = $"Error uploading file [{System.Web.HttpUtility.HtmlEncode(Path.GetFileName(contentDisposition.FileName.Value))}]: {ex.Message}" });
                }
                finally
                {
                    _logger.LogInformation($"Deleting the temp directory [{tempDir.FullName}]");
                    Directory.Delete(tempDir.FullName, true);
                }
            }

            section = await reader.ReadNextSectionAsync();
        }

        return BadRequest(new { Message = "Something went wrong with your request" });
    }

    /// <summary>
    /// Returns the info for the specified motec file
    /// </summary>
    /// <param name="id">The motec file ID to get</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetMotecFile(int id)
    {
        if (id < 1)
        {
            _logger.LogError($"Tried getting laps from file ID [{id}]");
            return BadRequest();
        }

        var motecFromDb = await _context.MotecFiles
            .AsNoTracking()
            .Include(m => m.Car)
            .Include(m => m.Track)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (motecFromDb is null)
        {
            _logger.LogError($"The motec file with id [{id}] could not be found");
            return NotFound();
        }

        return Ok(_mapper.Map<MotecFileDto>(motecFromDb));
    }

    /// <summary>
    /// Returns the list of laps from the specified motec file
    /// </summary>
    /// <param name="id">The motec file ID to get</param>
    /// <returns></returns>
    [HttpGet("laps/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetMotecFileLaps(int id)
    {
        if (id < 1)
        {
            _logger.LogError($"Tried getting laps from file ID [{id}]");
            return BadRequest();
        }

        var userValidation = await ValidateUser();
        if (userValidation.Result != null)
        {
            return userValidation.Result;
        }

        var motecFromDb = await _context.MotecFiles
            .AsNoTracking()
            .Include(m => m.Car)
            .Include(m => m.Track)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (motecFromDb is null)
        {
            _logger.LogError($"The motec file with id [{id}] could not be found");
            return NotFound();
        }

        if (!System.IO.File.Exists(Path.Combine(_storagePage, motecFromDb.FileLocation)))
        {
            _logger.LogError($"The motec file [{motecFromDb.FileLocation}] could not be found on disk");
            return NotFound(new { Message = "The motec file could not be opened" });
        }

        var tempDir = Directory.CreateDirectory(Path.Combine(_storagePage, $"TEMP-{Path.GetFileNameWithoutExtension(motecFromDb.FileLocation)}"));
        try
        {
            ZipFile.ExtractToDirectory(Path.Combine(_storagePage, motecFromDb.FileLocation), tempDir.FullName);
            var motecFile = await _parser.ParseFilesAsync(Directory.GetFiles(tempDir.FullName));
            var mapped = _mapper.Map<MotecLapDto>(motecFile);

            var comboAverage = await _context.AverageLaps
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.CarId == motecFromDb.CarId && a.TrackId == motecFromDb.TrackId);
            _logger.LogInformation($"Car + track average fastest lap: [{comboAverage!.AverageFastestLap}]");

            var classAverage = await _context.AverageLaps
                .AsNoTracking()
                .Include(a => a.Car)
                .Where(a => a.TrackId == motecFromDb.TrackId && a.Car.Class.Equals(motecFromDb.Car.Class))
                .AverageAsync(a => a.AverageFastestLap);
            _logger.LogInformation($"Class average fastest lap: [{classAverage}]");

            var classFastest = await _context.MotecFiles
                .AsNoTracking()
                .Include(m => m.Car)
                .Where(m => m.TrackId == motecFromDb.TrackId && m.Car.Class.Equals(motecFromDb.Car.Class))
                .MinAsync(m => m.FastestLap);
            _logger.LogInformation($"Class fastest lap: [{classFastest}]");

            mapped.CarTrackAverageLap = comboAverage!.AverageFastestLap;
            mapped.ClassAverageLap = classAverage;
            mapped.ClassBestLap = classFastest;

            await _auditRepo.PostAuditEvent(EventType.GetLaps, userValidation.User!.Id, $"Got laps for file [{motecFromDb.Id}]", motecFromDb.Id);

            return Ok(mapped);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(new { Message = "Could not find the motec file on disk" });
        }
        catch (MotecParseException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        finally
        {
            _logger.LogInformation($"Deleting the temp directory [{tempDir.FullName}]");
            Directory.Delete(tempDir.FullName, true);
        }
    }

    /// <summary>
    /// Downloads the specified motec file as a zip archive
    /// </summary>
    /// <param name="id">The motec file ID to download</param>
    /// <returns></returns>
    [HttpGet("download/{id}")]
    public async Task<ActionResult> DownloadFile(int id)
    {
        if (id < 1)
        {
            _logger.LogError($"Tried getting laps from file ID [{id}]");
            return BadRequest();
        }

        var userValidation = await ValidateUser();
        if (userValidation.Result != null)
        {
            return userValidation.Result;
        }

        var motecFromDb = await _context.MotecFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (motecFromDb is null)
        {
            _logger.LogError($"The motec file with id [{id}] could not be found");
            return NotFound();
        }

        if (!System.IO.File.Exists(Path.Combine(_storagePage, motecFromDb.FileLocation)))
        {
            _logger.LogError($"The motec file [{motecFromDb.FileLocation}] could not be found on disk");
            return NotFound("The motec files could not be found on disk");
        }

        await _auditRepo.PostAuditEvent(EventType.DownloadFile, userValidation.User!.Id, $"Downloaded file [{motecFromDb.Id}][{motecFromDb.FileLocation}]", motecFromDb.Id);

        return new PhysicalFileResult(Path.GetFullPath(Path.Combine(_storagePage, motecFromDb.FileLocation)), "application/x-zip-compressed");
    }

    /// <summary>
    /// Delets a motec file from the database. Only admins can delete files.
    /// </summary>
    /// <param name="id">The ID being deleted</param>
    /// <returns></returns>
    [HttpDelete("delete/{id}", Name = "DeleteMotecFile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteFile(int id)
    {
        if (id < 1)
        {
            _logger.LogError($"Tried getting laps from file ID [{id}]");
            return BadRequest();
        }

        var userValidation = await ValidateUser();
        if (userValidation.Result != null)
        {
            return userValidation.Result;
        }

        if (!userValidation.User!.Role.Equals("admin"))
        {
            _logger.LogError($"User with ID [{userValidation.User!.Id.ToString()}] is not an admin");
            return Unauthorized();
        }

        var motecFromDb = await _context.MotecFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (motecFromDb is null)
        {
            _logger.LogError($"The motec file with id [{id}] could not be found");
            return NotFound();
        }

        _context.MotecFiles.Remove(motecFromDb);
        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync();
        }
        var location = Path.Combine(_storagePage, motecFromDb.FileLocation);
        if (System.IO.File.Exists(location))
        {
            _logger.LogInformation($"Deleting file [{location}]");
            System.IO.File.Delete(location);
        }

        // recalculate the average now that there's one less file
        var average = await _context.AverageLaps.FirstOrDefaultAsync(a => a.CarId == motecFromDb.CarId && a.TrackId == motecFromDb.TrackId);
        _logger.LogInformation($"Existing average lap for car [{motecFromDb.CarId}] track [{motecFromDb.TrackId}] -- [{average!.AverageFastestLap}]");

        var existingLaps = await _context.MotecFiles
            .AsNoTracking()
            .Where(m => m.CarId == motecFromDb.CarId && m.TrackId == motecFromDb.TrackId)
            .Select(m => m.FastestLap)
            .ToListAsync();

        if (existingLaps is not null && existingLaps.Any())
        {
            _logger.LogInformation($"Found [{existingLaps.Count}] existing fastest laps");
            average.AverageFastestLap = existingLaps.Average();
            _logger.LogInformation($"New average laptime [{average.AverageFastestLap}]");
        }
        // deleted the only file, now no laps for that car/track
        else
        {
            _logger.LogInformation($"Removing the average lap for the car [{motecFromDb.CarId}] and track [{motecFromDb.TrackId}] because no entries exist");
            _context.AverageLaps.Remove(average);
        }

        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync();
        }

        return Ok();
    }

    /// <summary>
    /// Returns a collection containing the car + track for each motec file. Used for a chart.
    /// </summary>
    /// <returns></returns>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMotecStats()
    {
        var data = await _context.MotecFiles
            .AsNoTracking()
            .Include(m => m.Car)
            .Include(m => m.Track)
            .GroupBy(m => new { Car = m.Car.Name, CarId = m.CarId, Track = m.Track.Name, TrackId = m.TrackId })
            .Select(g => new { Car = g.Key.Car, CarId = g.Key.CarId, Track = g.Key.Track, TrackId = g.Key.TrackId, Count = g.Count() })
            .OrderBy(g => g.Track)
                .ThenBy(g => g.Car)
            .ToListAsync();
        
        return Ok(data);
    }

    /// <summary>
    /// Returns a collection containing the user for each motec file. Used for a chart.
    /// </summary>
    /// <returns></returns>
    [HttpGet("stats/users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMotecUserStats()
    {
        var data = await _context.MotecFiles
            .AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Car)
            .Include(m => m.Track)
            .GroupBy(m => new { Car = m.Car.Name, CarId = m.CarId, Track = m.Track.Name, TrackId = m.TrackId, User = m.User.ServerName, UserId = m.UserId })
            .Select(g => new { Car = g.Key.Car, CarId = g.Key.CarId, Track = g.Key.Track, TrackId = g.Key.TrackId, User = g.Key.User, UserId = g.Key.UserId, Count = g.Count() })
            .OrderBy(g => g.Track)
                .ThenBy(g => g.Car)
            .ToListAsync();
        
        return Ok(data);
    }

    /// <summary>
    /// Returns a collection containing the user for each motec file. Used for a chart.
    /// </summary>
    /// <returns></returns>
    [HttpGet("stats/users/laptimes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMotecUserLapStats()
    {
        var data = await _context.MotecFiles
            .AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Car)
            .Include(m => m.Track)
            .Where(m => m.TrackCondition.HasValue)
            .GroupBy(m => new { Car = m.Car.Name, CarId = m.CarId, Track = m.Track.Name, TrackId = m.TrackId, User = m.User.ServerName, UserId = m.UserId, TrackCondition = m.TrackCondition })
            .Select(g => new { Car = g.Key.Car, CarId = g.Key.CarId, Track = g.Key.Track, TrackId = g.Key.TrackId, User = g.Key.User, UserId = g.Key.UserId, TrackCondition = g.Key.TrackCondition!.ToString(), Laptime = g.Min(x => x.FastestLap) })
            .OrderBy(g => g.User)
                .ThenBy(g => g.Track)
                    .ThenBy(g => g.Car)
            .ToListAsync();
        
        return Ok(data);
    }

    /// <summary>
    /// Gets the average and fastest lap stats for each track and car
    /// </summary>
    /// <returns></returns>
    [HttpGet("stats/laps")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMotecLapStats()
    {
        var result = await _context.MotecFiles
            .Include(m => m.Car)
            .Include(m => m.Track)
            .Where(m => m.TrackCondition != null)
            .Join(
                _context.AverageLaps,
                motec => new { CarId = motec.CarId, TrackId = motec.TrackId, TrackCondition = motec.TrackCondition!.Value },
                avg => new { CarId = avg.CarId, TrackId = avg.TrackId, TrackCondition = avg.TrackCondition },
                (motec, avg) => new
                {
                    CarName = motec.Car.Name,
                    CarId = motec.CarId,
                    TrackName = motec.Track.Name,
                    TrackId = motec.TrackId,
                    TrackCondition = avg.TrackCondition,
                    AverageFastestLap = avg.AverageFastestLap,
                    FastestLap = motec.FastestLap
                }
            )
            .GroupBy(m => new { m.CarName, m.CarId, m.TrackName, m.TrackId, m.TrackCondition})
            .Select(m => new MotecStatDto
            {
                FastestLap = m.Min(x => x.FastestLap),
                Car = m.Key.CarName,
                CarId = m.Key.CarId,
                AverageFastestLap = m.Min(x => x.AverageFastestLap),
                TrackCondition = m.Key.TrackCondition.ToString(),
                Track = m.Key.TrackName,
                TrackId = m.Key.TrackId
            })
            .OrderBy(m => m.Track)
                .ThenBy(m => m.Car)
            .ToListAsync();
        
        return Ok(result);
    }

    /// <summary>
    /// Updates the comment of a motec file
    /// </summary>
    /// <param name="id">The file ID being updated</param>
    /// <param name="file">The post body containing the comment</param>
    /// <returns></returns>
    [HttpPut("{id}/comment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateFileComment(int id, [FromBody] MotecFileCommentDto file)
    {
        if (id < 1)
        {
            _logger.LogError($"Tried getting laps from file ID [{id}]");
            return BadRequest();
        }

        var userValidation = await ValidateUser();
        if (userValidation.Result != null)
        {
            return userValidation.Result;
        }

        if (file.Comment.Length > 256)
        {
            _logger.LogInformation($"The comment length [{file.Comment.Length}] is too long");
            return BadRequest(new { Message = "The comment length is too long" });
        }

        var motecFromDb = await _context.MotecFiles
            .FirstOrDefaultAsync(m => m.Id == id);

        if (motecFromDb is null)
        {
            _logger.LogError($"The motec file with id [{id}] could not be found");
            return NotFound();
        }

        if (!userValidation.User!.Role.Equals("admin") && motecFromDb.UserId != userValidation.User!.Id)
        {
            _logger.LogInformation($"Non-admin user [{userValidation.User!.Id}] tried commenting on motec file [{id}] which they didn't submit");
            return Unauthorized();
        }

        var regex = new Regex("(\n)\\1+");
        motecFromDb.Comment = regex.Replace(file.Comment.TrimEnd('\n'), "$1");
        await _context.SaveChangesAsync();
        await _auditRepo.PostAuditEvent(EventType.UpdateComment, userValidation.User!.Id, $"Set comment to [{file.Comment}]", motecFromDb.Id);

        return Ok();
    }

    /// <summary>
    /// Updates the track conditions of a motec file
    /// </summary>
    /// <param name="id">The file ID being updated</param>
    /// <param name="file">The post body containing the new track conditions</param>
    /// <returns></returns>
    [HttpPut("{id}/conditions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateFileTrackConditions(int id, [FromBody] MotecTrackConditionsDto file)
    {
        if (id < 1)
        {
            _logger.LogError($"Tried getting laps from file ID [{id}]");
            return BadRequest();
        }

        var userValidation = await ValidateUser();
        if (userValidation.Result != null)
        {
            return userValidation.Result;
        }

        if (!Enum.TryParse(typeof(TrackCondition), file.TrackConditions, out var trackCondition))
        {
            _logger.LogError($"The supplied condition is not valid");
            return BadRequest();
        }
        _logger.LogInformation($"Setting track condition to [{trackCondition}]");

        var motecFromDb = await _context.MotecFiles
            .FirstOrDefaultAsync(m => m.Id == id);

        if (motecFromDb is null)
        {
            _logger.LogError($"The motec file with id [{id}] could not be found");
            return NotFound();
        }

        if (!userValidation.User!.Role.Equals("admin") && motecFromDb.UserId != userValidation.User!.Id)
        {
            _logger.LogInformation($"Non-admin user [{userValidation.User!.Id}] tried editing the conditions on motec file [{id}] which they didn't submit");
            return Unauthorized();
        }

        motecFromDb.TrackCondition = (TrackCondition) trackCondition!;
        await _context.SaveChangesAsync();
        await _auditRepo.PostAuditEvent(EventType.UpdateTrackCondition, userValidation.User!.Id, $"Set track condition to [{file.TrackConditions}]", motecFromDb.Id);

        return Ok();
    }

    [HttpGet("{id}/notify")]
    public async Task<ActionResult> Notify(int id)
    {
        var userCookie = HttpContext.Request.Cookies.FirstOrDefault(c => c.Key.Equals("user"));
        if (!long.TryParse(JsonDocument.Parse(userCookie.Value).RootElement.GetProperty("Id").GetString(), out var userId))
        {
            _logger.LogError("Error parsing the user cookie");
            return Unauthorized();
        }

        var motecFromDb = await _context.MotecFiles
            .Include(m => m.Car)
            .Include(m => m.Track)
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (motecFromDb is null)
        {
            _logger.LogError($"The motec file with id [{id}] could not be found");
            return NotFound();
        }

        var anyFasterLaps = await _context.MotecFiles
            .AnyAsync(m => m.TrackId == motecFromDb.TrackId
                && m.TrackCondition == motecFromDb.TrackCondition
                && m.FastestLap < motecFromDb.FastestLap);
        
        await _discordNotifier.Notify(motecFromDb, JsonDocument.Parse(userCookie.Value).RootElement.GetProperty("Avatar").GetString(), anyFasterLaps);

        return Ok();
    }


    /// <summary>
    /// Updates the average lap time table for the given motec file's car and track
    /// </summary>
    /// <param name="id">The ID of the motec file who's car and track average fastest lap time are being updated</param>
    /// <returns></returns>
    [HttpPost("average")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateAverage()
    {
        var userValidation = await ValidateUser();
        if (userValidation.Result != null)
        {
            return userValidation.Result;
        }

        if (string.IsNullOrEmpty(_config.GetValue<string>("SQLITE_DATABASE")))
        {
            _logger.LogInformation("Removing all average laps from MYSQL database");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE AverageLaps;");
        }
        else
        {
            _logger.LogInformation("Removing all average laps from SQLite database");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM AverageLaps;");
        }
        await _context.SaveChangesAsync();

        var averageLaps = await _context.MotecFiles
            .Where(m => m.TrackCondition != null)
            .GroupBy(m => new { m.CarId, m.TrackId, m.TrackCondition })
            .Select(g => new AverageLap
            {
                CarId = g.Key.CarId,
                TrackId = g.Key.TrackId,
                TrackCondition = g.Key.TrackCondition!.Value,
                AverageFastestLap = g.Average(m => m.FastestLap)
            })
            .ToListAsync();

        _logger.LogInformation($"Recalculated [{averageLaps.Count}] average laps");
        
        await _context.AverageLaps.AddRangeAsync(averageLaps);
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

        return (null, user, userCookie);
    }

    private async Task<(ActionResult? result, IQueryable<Datastore.Models.MotecFile>? files)> GetMotecFiles(IEnumerable<int>? carIds, IEnumerable<int>? trackIds,
        IEnumerable<long>? userIds)
    {
        var files = _context.MotecFiles
            .AsNoTracking()
            .Include(m => m.Car)
            .Include(m => m.Track)
            .Include(m => m.User)
            .AsQueryable();

        if (carIds is not null && carIds.Count() > 0)
        {
            _logger.LogInformation($"Filtering motec files for cars [{string.Join(",", carIds)}]");
            var unknown = carIds.Except(await _context.Cars.AsNoTracking().Select(c => c.Id).ToListAsync());
            if (unknown.Any())
            {
                _logger.LogWarning($"At least of the car IDs is not found [{string.Join(",", unknown)}]");
                return (NotFound(), null);
            }
            files = files.Where(f => carIds.Contains(f.CarId));
        }

        if (trackIds is not null && trackIds.Count() > 0)
        {
            _logger.LogInformation($"Filtering motec files for tracks [{string.Join(",", trackIds)}]");
            var unknown = trackIds.Except(await _context.Tracks.AsNoTracking().Select(c => c.Id).ToListAsync());
            if (unknown.Any())
            {
                _logger.LogWarning($"At least of the track IDs is not found [{string.Join(",", unknown)}]");
                return (NotFound(), null);
            }
            files = files.Where(f => trackIds.Contains(f.TrackId));
        }

        if (userIds is not null && userIds.Count() > 0)
        {
            _logger.LogInformation($"Filtering motec files for users [{string.Join(",", userIds)}]");
            var unknown = userIds.Except(await _context.Users.AsNoTracking().Select(c => c.Id).ToListAsync());
            if (unknown.Any())
            {
                _logger.LogWarning($"At least of the user IDs is not found [{string.Join(",", unknown)}]");
                return (NotFound(), null);
            }
            files = files.Where(f => userIds.Contains(f.UserId));
        }

        return (null, files);
    }
}
