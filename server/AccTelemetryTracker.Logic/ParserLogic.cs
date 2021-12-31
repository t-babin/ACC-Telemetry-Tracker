using Microsoft.Extensions.Logging;

namespace AccTelemetryTracker.Logic;
public class ParserLogic : IParserLogic
{
    // filter out lap times that are too small, i.e. track cuts, not full laps, etc.
    private readonly Dictionary<string, int[]> _validTimes = new Dictionary<string, int[]>()
    {
        { "barcelona", new [] { 100, 120 } },
        { "brands_hatch", new [] { 80, 110 } },
        { "donington", new [] { 82, 110 } },
        { "hungaroring", new [] { 100, 120 } },
        { "imola", new [] { 99, 115 } },
        { "kyalami", new [] { 96, 120 } },
        { "laguna_seca", new [] { 78, 110 } },
        { "misano", new [] { 90, 120 } },
        { "monza", new [] { 103, 120 } },
        { "mount_panorama", new [] { 115, 135 } },
        { "nurburgring", new [] { 110, 130 } },
        { "paul_ricard", new [] { 90, 125 } },
        { "snetterton", new [] { 100, 130 } },
        { "spa-francorchamps", new [] { 130, 160 } },
        { "silverstone", new [] { 113, 130 } },
        { "oulton_park", new [] { 90, 120 } },
        { "suzuka", new [] { 115, 130 } },
        { "zandvoort", new [] { 92, 120 } },
        { "zolder", new [] { 85, 115 } }
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