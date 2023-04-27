namespace AccTelemetryTracker.Logic;
public interface IMotecParser
{
    /// <summary>
    /// Parses a MoTeC LDX file for all of the lap times
    /// </summary>
    /// <param name="path">The full path of the .ldx file being parsed</param>
    /// <returns>A collection of laps</returns>
    IEnumerable<MotecLap> ParseLaps(string? path);

    IEnumerable<MotecLap> ParseLaps(IEnumerable<string> files);

    /// <summary>
    /// Checks if the supplied path contains a valid .ldx MoTeC file
    /// </summary>
    /// <param name="path">The full path of the file being checked</param>
    /// <returns>If the file is a valid .ldx file or not</returns>
    bool IsValidLdx(string? path);

    /// <summary>
    /// Parses a .ld MoTeC file and returns an object with certain properties
    /// </summary>
    /// <param name="files">The file collection (.ld and .ldx) being parsed</param>
    /// <param name="fileName">The file name to save the entry to the database with</param>
    /// <param name="userId">The user ID that uploaded the file</param>
    /// <returns>A Task containing the MotecFile object</returns>
    Task<Datastore.Models.MotecFile> ParseMotecFileAsync(IEnumerable<string> files, string fileName, long userId);
}
