using AccTelemetryTracker.Api.Dto;
using AccTelemetryTracker.Datastore;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using AccTelemetryTracker.Api.Helpers;
using System.IO.Compression;
using AccTelemetryTracker.Logic;
using AccTelemetryTracker.Datastore.Models;

namespace AccTelemetryTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotecController : ControllerBase
{
    private readonly ILogger<MotecController> _logger;
    private readonly AccTelemetryTrackerContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly IParserLogic _parser;

    public MotecController(ILogger<MotecController> logger, AccTelemetryTrackerContext context, IMapper mapper, IConfiguration config, IParserLogic parser)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _config = config;
        _parser = parser;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll()
    {
        return Ok(_mapper.Map<IEnumerable<MotecFileDto>>(await _context.MotecFiles
            .Include(m => m.Car)
            .Include(m => m.Track)
            .ToListAsync())
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost(Name = "UploadFile")]
    [RequestSizeLimit(500_000_000)]
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

        var reader = new MultipartReader(mediaTypeHeader.Boundary.Value, request.Body);
        var section = await reader.ReadNextSectionAsync();

        while (section != null)
        {
            var hasContentDisposition = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
            if (hasContentDisposition && contentDisposition!.DispositionType.Equals("form-data") && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                _logger.LogInformation($"Got file [{System.Web.HttpUtility.HtmlEncode(Path.GetFileName(contentDisposition.FileName.Value))}]");
                var fileName = Path.GetRandomFileName();
                // TODO add env var for the file storage path
                var savePath = Path.Combine("files", $"{fileName}.zip");
                var tempDir = Directory.CreateDirectory(Path.Combine("files", $"TEMP-{fileName}"));

                try
                {
                    var validBytes = await FileHelper.ProcessStreamedFile(section, contentDisposition);
                    if (!validBytes.Any())
                    {
                        return BadRequest(new { Message = "Error occurred" });
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
                        return BadRequest(new { Message = "Error parsing the MoTeC files "});
                    }

                    var existingMotec = await _context.MotecFiles
                        .Include(m => m.Car)
                        .Include(m => m.Track)
                        .FirstOrDefaultAsync(m => m.SessionDate.Equals(motecFile.Date) && m.Car.Name.Equals(motecFile.Car) && m.Track.Name.Equals(motecFile.Track));
                    
                    if (existingMotec != null)
                    {
                        _logger.LogInformation($"Motec file already exists. Deleting the file [{savePath}]");
                        System.IO.File.Delete(savePath);
                        return BadRequest(new { Message = "This motec file has already been uploaded" });
                    }

                    var car = await _context.Cars.FirstOrDefaultAsync(c => c.Name.Equals(motecFile.Car));
                    var track = await _context.Tracks.FirstOrDefaultAsync(c => c.Name.Equals(motecFile.Track));
                    if (car == null)
                    {
                        car = new Car { Name = motecFile.Car };
                        _logger.LogInformation($"Creating new car [{car.Name}]");
                        _context.Cars.Add(car);
                        await _context.SaveChangesAsync();
                    }
                    if (track == null)
                    {
                        track = new Track { Name = motecFile.Track };
                        _logger.LogInformation($"Creating new car [{track.Name}]");
                        _context.Tracks.Add(track);
                        await _context.SaveChangesAsync();
                    }

                    _context.MotecFiles.Add(new Datastore.Models.MotecFile
                    {
                        TrackId = track.Id,
                        CarId = car.Id,
                        DateInserted = DateTime.Now,
                        NumberOfLaps = motecFile.Laps.Count(),
                        FileLocation = savePath,
                        FastestLap = motecFile.Laps.Min(l => l.LapTime),
                        SessionDate = motecFile.Date
                    });
                    await _context.SaveChangesAsync();

                    return Ok(motecFile);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, ex.Message);
                    return BadRequest(new { Message = ex.Message });
                }
                catch (MotecParseException ex)
                {
                    return BadRequest(new { Message = ex.Message });
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
}
