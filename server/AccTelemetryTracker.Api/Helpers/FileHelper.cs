using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Serilog;

namespace AccTelemetryTracker.Api.Helpers;

public class FileHelper
{
    private static readonly Serilog.ILogger _logger = Log.ForContext(typeof(FileHelper));
    
    private static readonly List<byte[]> _zipExtension = new List<byte[]> 
    {
        new byte[] { 0x50, 0x4B, 0x03, 0x04 },
        new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
        new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
        new byte[] { 0x50, 0x4B, 0x05, 0x06 },
        new byte[] { 0x50, 0x4B, 0x07, 0x08 },
        new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
    };

    /// <summary>
    /// Helper method to validate a file from an HTTP multipart request
    /// </summary>
    /// <param name="section">The multipart section being validated</param>
    /// <param name="contentDisposition">The content disposition header for the multipart section</param>
    /// <returns></returns>
    public static async Task<byte[]> ProcessStreamedFile(MultipartSection section, ContentDispositionHeaderValue contentDisposition)
    {
        try
        {
            using var memoryStream = new MemoryStream();

            await section.Body.CopyToAsync(memoryStream);

            if (memoryStream.Length == 0)
            {
                _logger.Error("The memory stream had no content");
                throw new IOException("The memory stream had no content");
            }
            else if (!IsValidFileExtensionAndSignature(contentDisposition.FileName.Value, memoryStream))
            {
                _logger.Error("The file extension is not valid");
                throw new IOException("The file extension is not valid");
            }
            else
            {
                return memoryStream.ToArray();
            }
        }
        catch (IOException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred when trying to save a file");
        }

        return Array.Empty<byte>();
    }

    /// <summary>
    /// Checks if the given file (and memory stream) have a valid file extension based on a specific signature
    /// </summary>
    /// <param name="fileName">The name of the file</param>
    /// <param name="data">The memory stream of the file's data</param>
    /// <returns>True if the extension matches the byte signature, false otherwise</returns>
    private static bool IsValidFileExtensionAndSignature(string fileName, Stream data)
    {
        if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
        {
            return false;
        }

        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(ext) || !ext.Equals(".zip"))
        {
            return false;
        }

        data.Position = 0;

        using (var reader = new BinaryReader(data))
        {
            var headerBytes = reader.ReadBytes(_zipExtension.Max(m => m.Length));
            return _zipExtension.Any(sig => headerBytes.Take(sig.Length).SequenceEqual(sig));
        }
    }
}