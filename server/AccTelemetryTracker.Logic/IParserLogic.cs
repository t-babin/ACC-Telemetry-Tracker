namespace AccTelemetryTracker.Logic;
public interface IParserLogic
{
    /// <summary>
    /// Parses the MoTeC .ld and .ldx files for the track, car, laps, and date
    /// </summary>
    /// <param name="files">A collection of files. One should be a .ld and the other should be a .ldx file</param>
    /// <returns>A task containing the parsed MoTeC file</returns>
    /// <exception cref="AccTelemetryTracker.Logic.MotecParseException">Thrown when a MoTeC file can't be parsed correctly</exception>
    /// <exception cref="System.IO.FileNotFoundException">Thrown when the supplied file paths can't be found</exception>
    Task<MotecFile> ParseFilesAsync(IEnumerable<string> files);
}