namespace AccMotec.Logic
{
    public interface IParserLogic
    {
        /// <summary>
        /// Parses the MoTeC .ld and .ldx files for the track, car, laps, and date
        /// </summary>
        /// <param name="ldxPath">The full file path of the .ldx file</param>
        /// <param name="ldPath">The full file path of the .ld file</param>
        /// <returns>A task containing the parsed MoTeC file</returns>
        Task<MotecFile> ParseFilesAsync(string ldxPath, string ldPath);
    }
}