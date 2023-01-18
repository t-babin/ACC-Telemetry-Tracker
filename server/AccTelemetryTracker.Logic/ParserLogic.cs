using Microsoft.Extensions.Logging;

namespace AccTelemetryTracker.Logic;
public class ParserLogic : IParserLogic
{
    // filter out lap times that are too small, i.e. track cuts, not full laps, etc.
    private readonly Dictionary<string, int[]> _validTimes = new Dictionary<string, int[]>()
    {
        { "barcelona", new [] { 100, 170 } },
        { "brands_hatch", new [] { 80, 160 } },
        { "cota", new [] { 120, 200 } },
        { "donington", new [] { 82, 160 } },
        { "hungaroring", new [] { 100, 170 } },
        { "imola", new [] { 99, 165 } },
        { "kyalami", new [] { 96, 170 } },
        { "indianapolis", new [] { 93, 160 } },
        { "laguna_seca", new [] { 78, 160 } },
        { "misano", new [] { 90, 170 } },
        { "monza", new [] { 103, 170 } },
        { "mount_panorama", new [] { 115, 185 } },
        { "nurburgring", new [] { 110, 180 } },
        { "paul_ricard", new [] { 90, 175 } },
        { "snetterton", new [] { 100, 180 } },
        { "spa-francorchamps", new [] { 130, 210 } },
        { "silverstone", new [] { 113, 180 } },
        { "oulton_park", new [] { 90, 170 } },
        { "suzuka", new [] { 115, 180 } },
        { "watkins_glen", new [] { 101, 170 } },
        { "zandvoort", new [] { 92, 170 } },
        { "zolder", new [] { 85, 165 } }
    };

    private readonly Dictionary<(DateTime Start, DateTime End), string> _gameVersions = new Dictionary<(DateTime Start, DateTime End), string>()
    {
        { (new DateTime(), new DateTime(2021, 06, 29)), "pre-1.7.12" },
        { (new DateTime(2021, 06, 30), new DateTime(2021, 08, 22)), "1.7.12" },
        { (new DateTime(2021, 08, 23), new DateTime(2021, 09, 08)), "1.7.13" },
        { (new DateTime(2021, 09, 09), new DateTime(2021, 10, 06)), "1.7.14" },
        { (new DateTime(2021, 10, 07), new DateTime(2021, 11, 23)), "1.7.15" },
        { (new DateTime(2021, 11, 24), new DateTime(2021, 11, 24)), "1.8.0"  },
        { (new DateTime(2021, 11, 25), new DateTime(2021, 11, 25)), "1.8.1"  },
        { (new DateTime(2021, 11, 26), new DateTime(2021, 12, 01)), "1.8.2"  },
        { (new DateTime(2021, 12, 02), new DateTime(2021, 12, 02)), "1.8.5"  },
        { (new DateTime(2021, 12, 03), new DateTime(2021, 12, 08)), "1.8.6"  },
        { (new DateTime(2021, 12, 09), new DateTime(2021, 12, 14)), "1.8.7"  },
        { (new DateTime(2021, 12, 15), new DateTime(2022, 01, 06)), "1.8.8"  },
        { (new DateTime(2022, 01, 07), new DateTime(2022, 01, 17)), "1.8.9"  },
        { (new DateTime(2022, 01, 18), new DateTime(2022, 02, 06)), "1.8.10" },
        { (new DateTime(2022, 02, 07), new DateTime(2022, 03, 22)), "1.8.11" },
        { (new DateTime(2022, 03, 23), new DateTime(2022, 04, 01)), "1.8.12" },
        { (new DateTime(2022, 04, 02), new DateTime(2022, 04, 27)), "1.8.13" },
        { (new DateTime(2022, 04, 28), new DateTime(2022, 06, 29)), "1.8.14" },
        { (new DateTime(2022, 06, 30), new DateTime(2022, 07, 05)), "1.8.15" },
        { (new DateTime(2022, 07, 06), new DateTime(2022, 07, 12)), "1.8.16" },
        { (new DateTime(2022, 07, 13), new DateTime(2022, 08, 21)), "1.8.17" },
        { (new DateTime(2022, 08, 22), new DateTime(2022, 11, 15)), "1.8.18" },
        { (new DateTime(2022, 11, 16), new DateTime(2022, 12, 26)), "1.8.19" },
        { (new DateTime(2022, 12, 27), new DateTime(2023, 01, 16)), "1.8.20" },
        { (new DateTime(2023, 01, 17), DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59)), "1.8.21" },
    };
    private readonly IMotecParser _parser;
    private readonly ILogger<ParserLogic> _logger;

    public ParserLogic(IMotecParser parser, ILogger<ParserLogic> logger)
    {
        _parser = parser;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<MotecFile> ParseFilesAsync(IEnumerable<string> files)
    {
        MotecFile motecFile;
        if (files.Count() != 2)
        {
            _logger.LogError("More than two files were provided");
            throw new IOException("More than two files were provided");
        }
        var ldxPath = ValidateExtension(files, ".ldx");
        var ldPath = ValidateExtension(files, ".ld");

        try
        {
            if (!_parser.IsValidLdx(ldxPath))
            {
                _logger.LogError($"The file [{ldxPath}] is not a valid ldx file.");
                throw new MotecParseException($"The file [{ldxPath}] is not a valid ldx file.");
            }

            motecFile = await _parser.ParseMotecFileAsync(ldPath);
            motecFile.Laps = _parser.ParseLaps(ldxPath);
        }
        catch (MotecParseException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }

        motecFile.Laps = motecFile.Laps.Where(l => l.LapTime > _validTimes[motecFile.Track.ToLower().Replace(" ", "_")][0]
            && l.LapTime < _validTimes[motecFile.Track.ToLower().Replace(" ", "_")][1]);
        motecFile.GameVersion = _gameVersions.FirstOrDefault(g => motecFile.Date.Date >= g.Key.Start && motecFile.Date.Date <= g.Key.End).Value;
        _logger.LogInformation($"Parsed [{motecFile.Laps.Count()}] valid laps from the file [{ldxPath}]");

        if (!motecFile.Laps.Any())
        {
            _logger.LogError($"No valid laps were found in the .ldx file [{ldxPath}] for the track [{motecFile.Track}]");
            throw new MotecParseException($"No valid laps were found in the .ldx file [{ldxPath}] for the track [{motecFile.Track}]");
        }

        return motecFile;
    }

    public void GetGameVersion(IEnumerable<Datastore.Models.MotecFile> files)
    {
        foreach (var file in files)
        {
            file.GameVersion = _gameVersions.FirstOrDefault(g => file.SessionDate.Date >= g.Key.Start && file.SessionDate.Date <= g.Key.End).Value;
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
