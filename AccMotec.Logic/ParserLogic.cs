namespace AccMotec.Logic
{
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

        // TODO logging
        public ParserLogic(IMotecParser parser)
        {
            _parser = parser;
        }

        public async Task<MotecFile> ParseFilesAsync(string ldxPath, string ldPath)
        {
            MotecFile motecFile;
            try
            {
                if (!_parser.IsValidLdx(ldxPath))
                {
                    return null!;
                }

                motecFile = await _parser.ParseMotecFileAsync(ldPath);
                motecFile.Laps = _parser.ParseLaps(ldxPath);
            }
            catch (MotecParseException)
            {
                return null!;
            }
            catch (FileNotFoundException)
            {
                return null!;
            }
            catch (Exception)
            {
                return null!;
            }

            motecFile.Laps = motecFile.Laps.Where(l => l.LapTime > _minTimes[motecFile.Track.ToLower()]);

            if (!motecFile.Laps.Any())
            {
                return null!;
            }

            return motecFile;
        }
    }
}