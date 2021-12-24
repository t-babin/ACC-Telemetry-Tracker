using Microsoft.Extensions.Logging;

namespace AccTelemetryTracker.Logic;
public class ParserLogic : IParserLogic
{
    // filter out lap times that are too small, i.e. track cuts, not full laps, etc.
    private readonly Dictionary<string, int> _minTimes = new Dictionary<string, int>()
        {
            { "barcelona", 100 },
            { "brands_hatch", 80 },
            { "donington", 82 },
            { "hungaroring", 100 },
            { "imola", 95 },
            { "kyalami", 96 },
            { "laguna_seca", 78 },
            { "misano", 90 },
            { "monza", 103 },
            { "mount_panorama", 115 },
            { "nurburgring", 110 },
            { "paul_ricard", 90 },
            { "snetterton", 100 },
            { "spa", 130 },
            { "silverstone", 113 },
            { "oulton_park", 90 },
            { "suzuka", 115 },
            { "zandvoort", 92 },
            { "zolder", 85}
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

        motecFile.Laps = motecFile.Laps.Where(l => l.LapTime > _minTimes[motecFile.Track.ToLower()]);
        _logger.LogInformation($"Parsed [{motecFile.Laps.Count()}] valid laps from the file [{ldxPath}]");

        if (!motecFile.Laps.Any())
        {
            _logger.LogError($"No valid laps were found in the .ldx file [{ldxPath}] for the track [{motecFile.Track}]");
            throw new MotecParseException($"No valid laps were found in the .ldx file [{ldxPath}] for the track [{motecFile.Track}]");
        }

        return motecFile;
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
