using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using AccTelemetryTracker.Datastore;
using AccTelemetryTracker.Datastore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AccTelemetryTracker.Logic;
public class MotecParser : IMotecParser
{
    private readonly ILogger<MotecParser> _logger;
    private readonly IServiceScopeFactory _serviceProvider;
    public MotecParser(ILogger<MotecParser> logger, IServiceScopeFactory serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public bool IsValidLdx(string? path)
    {
        var isValid = true;
        ValidatePath(path, ".ldx");

        var settings = new XmlReaderSettings();
        settings.Schemas.Add(string.Empty, "MotecLdx.xsd");
        settings.ValidationType = ValidationType.Schema;
        var validationHandler = new ValidationEventHandler(ValidateXsd);

        using var reader = XmlReader.Create(path!, settings);
        var document = new XmlDocument();
        try
        {
            document.Load(reader);
            document.Validate(validationHandler);
        }
        catch (XmlSchemaValidationException ex)
        {
            isValid = false;
            _logger.LogError("Error when validating the LDX file against the XSD schema");
            _logger.LogError(ex, ex.Message);
        }

        return isValid;
    }

    /// <inheritdoc />
    public IEnumerable<MotecLap> ParseLaps(string? path)
    {
        using (var reader = File.OpenText(path!))
        {
            var root = XDocument.Load(reader, LoadOptions.None).Root;
            if (root == null)
            {
                _logger.LogError("Couldn't load the LDX file");
                throw new XmlException("Couldn't load the LDX file");
            }

            var laps = root
                .Descendants("MarkerGroup")
                .Descendants("Marker")
                .Select(d => new MotecLap
                {
                    LapNumber = int.Parse(d.Attribute("Name")?.Value?.Split(", ")?[0] ?? "0"),
                    LapTime = Double.Parse(d.Attribute("Time")?.Value ?? "0", NumberStyles.Float) * 1e-6,
                    SessionTime = Double.Parse(d.Attribute("Time")?.Value ?? "0", NumberStyles.Float) * 1e-6
                })
                .OrderBy(m => m.LapNumber)
                .ToList();

            if (laps.Count() > 1)
            {
                // Loop through each lap and update the laptime based on the current session time that passed
                for (int i = 1; i < laps.Count(); i++)
                {
                    laps[i].LapTime = laps[i].LapTime - laps[i - 1].SessionTime;
                }
            }

            return laps;
        }
    }

    public IEnumerable<MotecLap> ParseLaps(IEnumerable<string> files)
    {
        ValidateFiles(files, out var ldxPath, out var ldPath);
        return ParseLaps(ldxPath);
    }

    /// <inheritdoc />
    public async Task<Datastore.Models.MotecFile> ParseMotecFileAsync(IEnumerable<string> files, string fileName, long userId)
    {
        ValidateFiles(files, out var ldxPath, out var ldPath);
        var resultFromFile = await ParseMotecContent(ldPath);

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AccTelemetryTrackerContext>();
        var validCars = await dbContext.Cars.AsNoTracking().ToListAsync();
        var validTracks = await dbContext.Tracks.AsNoTracking().ToListAsync();
        var gameVersions = await dbContext.GameVersions
            .AsNoTracking()
            .Select(g => new GameVersion { StartDate = g.StartDate, VersionNumber = g.VersionNumber, EndDate = g.EndDate.HasValue ? g.EndDate : DateTime.MaxValue })
            .ToListAsync();

        if (!validTracks.Any(t => t.MotecName.Equals(resultFromFile.Track)) && !validTracks.Any(t => t.Name.Equals(resultFromFile.Track)))
        {
            _logger.LogError($"The track [{resultFromFile.Track}] is not valid");
            throw new MotecParseException($"The track [{resultFromFile.Track}] is not valid");
        }
        var track = validTracks.First(t => t.MotecName.Equals(resultFromFile.Track) || t.Name.Equals(resultFromFile.Track));

        if (!validCars.Any(c => c.MotecName.Equals(resultFromFile.Car)))
        {
            _logger.LogError($"The Car [{resultFromFile.Car}] is not valid");
            throw new MotecParseException($"The Car [{resultFromFile.Car}] is not valid");
        }
        var car = validCars.First(c => c.MotecName.Equals(resultFromFile.Car));

        var laps = ParseLaps(ldxPath).Where(l => l.LapTime > track.MinLapTime && l.LapTime <= track.MaxLapTime);

        var existingMotec = await dbContext.MotecFiles
            .Include(m => m.Car)
            .Include(m => m.Track)
            .FirstOrDefaultAsync(m => m.SessionDate.Equals(resultFromFile.Date) && m.Car.MotecName.Equals(resultFromFile.Car) && m.Track.MotecName.Equals(resultFromFile.Track));

        if (existingMotec != null)
        {
            throw new MotecFileExistsException($"The motec file on the track {resultFromFile.Track} with the car {resultFromFile.Car} on the date {resultFromFile.Date.ToShortDateString()} already exists");
        }

        var motecFile = new Datastore.Models.MotecFile
        {
            TrackId = track.Id,
            CarId = car.Id,
            DateInserted = DateTime.Now,
            NumberOfLaps = laps.Count(),
            FileLocation = $"{fileName}.zip",
            FastestLap = laps.Min(l => l.LapTime),
            SessionDate = resultFromFile.Date,
            UserId = userId,
            GameVersion = gameVersions.FirstOrDefault(g => resultFromFile.Date.Date >= g.StartDate && resultFromFile.Date.Date <= g.EndDate)!.VersionNumber
        };
        dbContext.MotecFiles.Add(motecFile);

        await dbContext.SaveChangesAsync();

        return await dbContext.MotecFiles.AsNoTracking().Include(m => m.Car).Include(m => m.Track).Include(m => m.User).FirstAsync(m => m.Id == motecFile.Id);
    }

    private void ValidateFiles(IEnumerable<string> files, out string ldxPath, out string ldPath)
    {
        if (files.Count() != 2)
        {
            _logger.LogError("More than two files were provided");
            throw new IOException("More than two files were provided");
        }
        ldxPath = ValidateExtension(files, ".ldx");
        ldPath = ValidateExtension(files, ".ld");
        if (!IsValidLdx(ldxPath))
        {
            _logger.LogError($"The file [{ldxPath}] is not a valid ldx file.");
            throw new MotecParseException($"The file [{ldxPath}] is not a valid ldx file.");
        }

        ValidatePath(ldPath, ".ld");
        ValidatePath(ldxPath, ".ldx");
    }

    /// <summary>
    /// Parses the .ld Motec file for the session date, car, and track
    /// </summary>
    /// <returns></returns>
    private async Task<(DateTime Date, string Car, string Track)> ParseMotecContent(string? path)
    {
        var car = string.Empty;
        var track = string.Empty;
        var date = DateTime.MinValue;
        // "@H,4>?\u001f\u0001@B\u000fD\u001fADL?\u0001??%07/12/202117:59:12Kyalami??\fcR8 LMS EVOP
        // "@H,4>?\u001f\u0001@B\u000fD\u001fADL?\u0001??%02/12/202121:13:58Imola??\fc991ii GT3 R EVOU
        using (var stream = new FileStream(path!, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
        {
            var buffer = new byte[0x1000];
            int numRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (numRead > 0)
            {
                _logger.LogInformation($"Read [{numRead}] bytes from the file");
                var text = Encoding.ASCII.GetString(buffer, 0, numRead).Replace("\0", "");
                var parts = text.Split("??").Skip(1).ToArray();
                if (DateTime.TryParseExact(parts[0][1..19], "dd/MM/yyyyHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    if (date.Equals(default(DateTime)))
                    {
                        _logger.LogError("Unable to properly parse the file's datetime");
                        throw new MotecParseException("Unable to properly parse the file's datetime");
                    }
                }
                else
                {
                    _logger.LogError("Unable to properly parse the file's datetime");
                    throw new MotecParseException("Unable to properly parse the file's datetime");
                }
                track = parts[0][19..];
                car = parts[1].Replace("\fc", "")[0..^2];
            }
            else
            {
                _logger.LogError($"Unable to read the first 1000 file bytes of [{path}]");
                throw new MotecParseException($"Unable to read the first 1000 file bytes of [{path}]");
            }
        }
        return (date, car, track);
    }

    /// <summary>
    /// Validates if a file path exists or not. Throws an exception if not exists
    /// </summary>
    /// <param name="path">The path of the file being checked</param>
    private void ValidatePath(string? path, string extension)
    {
        if (string.IsNullOrEmpty(path))
        {
            _logger.LogError("The path was not provided");
            throw new FileNotFoundException("The path was not provided");
        }

        if (!File.Exists(path))
        {
            _logger.LogError($"The file [{path}] does not exist");
            throw new FileNotFoundException($"The file [{path}] does not exist");
        }

        if (!Path.GetExtension(path)?.Equals(extension) ?? true)
        {
            _logger.LogError($"The file extension [{Path.GetExtension(path)}] is not valid");
            throw new FileNotFoundException($"The file extension [{Path.GetExtension(path)}] is not valid");
        }
    }

    private void ValidateXsd(object? sender, ValidationEventArgs e)
    {
        if (e.Severity == XmlSeverityType.Error || e.Severity == XmlSeverityType.Warning)
        {
            // isValid = false;
            _logger.LogError($"invalid schema [{e.Message}]");
        }
    }

    private string ValidateExtension(IEnumerable<string> files, string extension)
    {
        if (!files.Any(f => Path.GetExtension(f)?.Equals(extension) ?? false))
        {
            _logger.LogError($"No file with the extension [{extension}] was provided");
            throw new FileNotFoundException($"No file with the extension [{extension}] was provided");
        }

        return files.FirstOrDefault(f => Path.GetExtension(f).Equals(extension))!;
    }
}
