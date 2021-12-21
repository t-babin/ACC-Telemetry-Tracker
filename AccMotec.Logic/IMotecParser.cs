namespace AccMotec.Logic;
public interface IMotecParser
{
    /// <summary>
    /// Parses a MoTeC LDX file for all of the lap times
    /// </summary>
    /// <param name="path">The full path of the .ldx file being parsed</param>
    /// <returns>A collection of laps</returns>
    IEnumerable<MotecLap> ParseLaps(string? path);

    /// <summary>
    /// Checks if the supplied path contains a valid .ldx MoTeC file
    /// </summary>
    /// <param name="path">The full path of the file being checked</param>
    /// <returns>If the file is a valid .ldx file or not</returns>
    bool IsValidLdx(string? path);

    /// <summary>
    /// Parses a .ld MoTeC file and returns an object with certain properties
    /// </summary>
    /// <param name="path">The full path of the .ld file being parsed</param>
    /// <returns>A Task containing the MotecFile object</returns>
    Task<MotecFile> ParseMotecFileAsync(string? path);
}
