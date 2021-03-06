using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.Extensions.Logging;

namespace AccTelemetryTracker.Logic;
public class MotecParser : IMotecParser
{
    private readonly ILogger<MotecParser> _logger;
    public MotecParser(ILogger<MotecParser> logger)
    {
        _logger = logger;
    }

    private readonly Dictionary<string, string> _validCars = new Dictionary<string, string>()
    {
        // GT3
        { "V12 Vantage GT3", "GT3"},
        { "V8 Vantage GT3", "GT3"},
        { "R8 LMS", "GT3" },
        { "R8 LMS EVO", "GT3" },
        { "R8 LMS EVO II", "GT3" },
        { "Continental 16", "GT3"},
        { "Continental 18", "GT3"},
        { "M4 GT3", "GT3"},
        { "M6 GT3", "GT3"},
        { "Emil Frey G3", "GT3"},
        { "488 GT3", "GT3"},
        { "488 GT3 Evo", "GT3"},
        { "NSX GT3", "GT3"},
        { "NSX GT3 EVO", "GT3"},
        { "Huracan GT3", "GT3"},
        { "Huracan GT3 EVO", "GT3"},
        { "RC F GT3", "GT3"},
        { "McLaren 650S GT3", "GT3"},
        { "McLaren 720S GT3", "GT3"},
        { "AMG GT3", "GT3"},
        { "AMG GT3 Evo", "GT3"},
        { "GTR GT3 17", "GT3"},
        { "GTR GT3 18", "GT3"},
        { "991 GT3 R 2016", "GT3" },
        { "991ii GT3 R EVO", "GT3" },
        { "Gallardo REX", "GT3" },

        // GT4
        { "A110 GT4", "GT4" },
        { "V8 Vantage GT4", "GT4" },
        { "R8 GT4", "GT4" },
        { "M4 GT4", "GT4" },
        { "Camaro GT4R", "GT4" },
        { "G55 GT4", "GT4" },
        { "XBOW GT4", "GT4" },
        { "GranTurismo MC GT4", "GT4" },
        { "570S GT4", "GT4" },
        { "AMG GT4", "GT4" },
        { "718 Cayman GT4 MR", "GT4" },

        // CUP
        { "991II GT3 Cup", "Cup" },
        { "992 GT3 Cup", "Cup" },

        // Super Trofeo
        { "Huracan ST", "ST" },
        { "Huracan ST Evo2", "ST" },

        // Challenge
        { "488 Challenge Evo", "CHL" },

        // TCX
        { "M2 CS Racing", "TCX" },
    };

    private readonly Dictionary<string, string> _validTracks = new Dictionary<string, string>()
    {
        { "barcelona", "Barcelona" },
        { "brands_hatch", "Brands Hatch" },
        { "cota", "Circuit of the Americas" },
        { "donington", "Donington" },
        { "hungaroring", "Hungaroring" },
        { "imola", "Imola" },
        { "indianapolis", "Indianapolis" },
        { "kyalami", "Kyalami" },
        { "laguna_seca", "Laguna Seca" },
        { "misano", "Misano" },
        { "monza", "Monza" },
        { "mount_panorama", "Mount Panorama" },
        { "nurburgring", "Nurburgring" },
        { "paul_ricard", "Paul Ricard" },
        { "snetterton", "Snetterton" },
        { "spa", "Spa-Francorchamps" },
        { "silverstone", "Silverstone" },
        { "oulton_park", "Oulton Park" },
        { "suzuka", "Suzuka" },
        { "watkins_glen", "Watkins Glen" },
        { "zandvoort", "Zandvoort" },
        { "zolder", "Zolder" }
    };

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
        ValidatePath(path, ".ldx");

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

    /// <inheritdoc />
    public async Task<MotecFile> ParseMotecFileAsync(string? path)
    {
        ValidatePath(path, ".ld");

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
                if (DateTime.TryParseExact(parts[0][1..19], "dd/MM/yyyyHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
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
                var track = parts[0][19..];
                if (!_validTracks.Any(t => t.Key.Equals(track.ToLower())))
                {
                    _logger.LogError($"The track [{track}] is not valid");
                    throw new MotecParseException($"The track [{track}] is not valid");
                }
                var car = parts[1].Replace("\fc", "")[0..^2];
                if (!_validCars.Keys.Any(c => c.Equals(car)))
                {
                    _logger.LogError($"The car [{car}] is not valid");
                    throw new MotecParseException($"The car [{car}] is not valid");
                }
                return new MotecFile { Car = car, CarClass = _validCars[car], Track = _validTracks[track.ToLower()], Date = date };
            }
            else
            {
                _logger.LogError($"Unable to read the first 1000 file bytes of [{path}]");
                throw new MotecParseException($"Unable to read the first 1000 file bytes of [{path}]");
            }
        }
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
}
