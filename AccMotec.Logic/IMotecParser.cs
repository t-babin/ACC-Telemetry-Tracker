namespace AccMotec.Logic;
public interface IMotecParser
{
    MotecFile? ParseFile(string? path);

    /// <summary>
    /// Checks if the supplied path contains a valid .ldx MoTeC file
    /// </summary>
    /// <param name="path">The full path of the file being checked</param>
    /// <returns>If the file is a valid .ldx file or not</returns>
    bool IsValidLdx(string? path);
}
