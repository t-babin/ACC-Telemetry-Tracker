using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace AccMotec.Logic
{
    public class MotecParser : IMotecParser
    {
        // TODO logging
        public MotecParser()
        { }

        // TODO get the rest of the cars
        private readonly string[] _validCars = new []
        {
            "R8 LMS EVO",
            "991ii GT3 R EVO"
        };

        private readonly string[] _validTracks = new []
        {
            "barcelona",
            "brands_hatch",
            "donington",
            "hungaroring",
            "imola",
            "kyalami",
            "laguna_seca",
            "misano",
            "monza",
            "mount_panorama",
            "nurburgring",
            "paul_ricard",
            "snetterton",
            "spa",
            "silverstone",
            "oulton_park",
            "suzuka",
            "zandvoort",
            "zolder"
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

            var reader = XmlReader.Create(path!, settings);
            var document = new XmlDocument();
            try
            {
                document.Load(reader);
                document.Validate(validationHandler);
            }
            catch (XmlSchemaValidationException ex)
            {
                isValid = false;
                Console.WriteLine(ex.Message);
            }

            return isValid;
        }

        /// <inheritdoc />
        public IEnumerable<MotecLap> ParseLaps(string? path)
        {
            ValidatePath(path, ".ldx");

            using var reader = File.OpenText(path!);
            var root = XDocument.Load(reader, LoadOptions.None).Root;
            if (root == null)
            {
                throw new XmlException("Couldn't load the LDX file");
            }

            var laps = root
                .Descendants("MarkerGroup")
                .Descendants("Marker")
                .Select(d => new MotecLap {
                    LapNumber = int.Parse(d.Attribute("Name")?.Value?.Split(", ")?[0] ?? "0"),
                    LapTime = Double.Parse(d.Attribute("Time")?.Value ?? "0", NumberStyles.Float) * 1e-6,
                    SessionTime = Double.Parse(d.Attribute("Time")?.Value ?? "0", NumberStyles.Float) * 1e-6 })
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

        /// <inheritdoc />
        public async Task<MotecFile> ParseMotecFileAsync(string? path)
        {
            ValidatePath(path, ".ld");

            // "@H,4>?\u001f\u0001@B\u000fD\u001fADL?\u0001??%07/12/202117:59:12Kyalami??\fcR8 LMS EVOP
            // "@H,4>?\u001f\u0001@B\u000fD\u001fADL?\u0001??%02/12/202121:13:58Imola??\fc991ii GT3 R EVOU
            using var stream = new FileStream(path!, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            var buffer = new byte[0x1000];
            int numRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (numRead > 0)
            {
                var text = Encoding.ASCII.GetString(buffer, 0, numRead).Replace("\0", "");
                var parts = text.Split("??").Skip(1).ToArray();
                if (DateTime.TryParseExact(parts[0][1..19], "dd/MM/yyyyHH:mm:ss", new CultureInfo("en-US"), DateTimeStyles.None, out var date))
                {
                    if (date.Equals(default(DateTime)))
                    {
                        throw new MotecParseException("Unable to properly parse the file's datetime");
                    }
                }
                else
                {
                    throw new MotecParseException("Unable to properly parse the file's datetime");
                }
                var track = parts[0][19..];
                if (!_validTracks.Any(t => t.Equals(track.ToLower())))
                {
                    throw new MotecParseException($"The track [{track}] is not valid");
                }
                var car = parts[1].Replace("\fc", "")[0..^2];
                if (!_validCars.Any(c => c.Equals(car)))
                {
                    throw new MotecParseException($"The car [{car}] is not valid");
                }
                return new MotecFile { Car = car, Track = track, Date = date };
            }
            else
            {
                throw new MotecParseException("Unable to read the first 1000 file bytes");
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
                throw new FileNotFoundException("The path was not provided");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"The file [{path}] does not exist");
            }

            if (!Path.GetExtension(path)?.Equals(extension) ?? true)
            {
                throw new FileNotFoundException($"The file extension [{Path.GetExtension(path)}] is not valid");
            }
        }

        private void ValidateXsd(object? sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error || e.Severity == XmlSeverityType.Warning)
            {
                // isValid = false;
                Console.WriteLine($"invalid schema [{e.Message}]");
            }
        }
    }
}