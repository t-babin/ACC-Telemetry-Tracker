using System.Diagnostics.CodeAnalysis;

namespace AccTelemetryTracker.Api.Dto;

public class UserDto
{
    [AllowNull]
    public string Id { get; set; }

    [AllowNull]
    public string Username { get; set; }

    [AllowNull]
    public string ServerName { get; set; }

    public bool IsValid { get; set; }

    [AllowNull]
    public string Role { get; set; }

    public DateTime SignupDate { get; set; }

    public int FileUploadCount { get; set; }
}

public class UserCollectionDto
{
    public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();
}
