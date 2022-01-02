using System.Net.Http.Headers;
using System.Text.Json;
using AccTelemetryTracker.Datastore;
using AccTelemetryTracker.Datastore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccTelemetryTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly AccTelemetryTrackerContext _context;
    private readonly IAuditRepository _auditRepo;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _frontendUrl;
    private readonly IConfiguration _config;
    public AuthController(ILogger<AuthController> logger, AccTelemetryTrackerContext context, IAuditRepository auditRepo, IConfiguration config)
    {
        _logger = logger;
        _context = context;
        _auditRepo = auditRepo;
        _config = config;

        _clientId = config.GetValue<string>("DISCORD_CLIENT_ID");
        _clientSecret = config.GetValue<string>("DISCORD_CLIENT_SECRET");
        _frontendUrl = config.GetValue<string>("FRONTEND_URL");
    }

    [HttpGet("callback")]
    public async Task<ActionResult> AuthCallback([FromQuery] string code)
    {
        using (var client = new HttpClient())
        {
            var requestDictionary = new Dictionary<string, string>()
            {
                { "code", code },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "authorization_code" },
                { "redirect_uri", $"{_frontendUrl}/auth" },
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token")
            {
                Content = new FormUrlEncodedContent(requestDictionary)
            };

            try
            {
                var response = await client.SendAsync(request);
                var authContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation(authContent);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Got token from discord API");
                var asJson = JsonDocument.Parse(authContent).RootElement;
                var token = asJson.GetProperty("access_token").GetString();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var userInfoRequest = await client.GetAsync("https://discord.com/api/users/@me");
                var userInfoResponse = await userInfoRequest.Content.ReadAsStringAsync();
                var userInfoJson = JsonDocument.Parse(userInfoResponse).RootElement;

                var guildId = _config.GetValue<string>("DISCORD_GUILD_ID");
                var serverName = userInfoJson.GetProperty("username").GetString();
                if (string.IsNullOrEmpty(guildId))
                {
                    _logger.LogInformation("Not checking guild membership");
                }
                else
                {
                    _logger.LogInformation($"Checking guild membership for the guild [{guildId}]");
                    var guildRequest = await client.GetAsync("https://discord.com/api/users/@me/guilds");
                    var guildResponse = await guildRequest.Content.ReadAsStringAsync();
                    var guildJson = JsonDocument.Parse(guildResponse).RootElement;
                    var inGuild = guildJson.EnumerateArray().AsEnumerable().Any(j => j.GetProperty("id").GetString()?.Equals(guildId) ?? false);
                    if (!inGuild)
                    {
                        _logger.LogWarning($"The user [{userInfoJson.GetProperty("id").GetString()}] is not in the guild [{guildId}]");
                        return Unauthorized();
                    }

                    var guildMemberRequest = await client.GetAsync($"https://discord.com/api/users/@me/guilds/{guildId}/member");
                    var guildMemberResponse = await guildMemberRequest.Content.ReadAsStringAsync();
                    var guildMemberJson = JsonDocument.Parse(guildMemberResponse).RootElement;
                    serverName = guildMemberJson.GetProperty("nick").GetString();

                    // TODO check if user is in a certain role maybe
                    // var guildRoleRequest = await client.GetAsync($"https://discord.com/api/guilds/{guildId}/roles");
                }

                var userId = long.Parse(userInfoJson.GetProperty("id").GetString() ?? "0L");
                if (userId == 0)
                {
                    _logger.LogWarning("Unable to properly parse the user info - couldn't convert ID");
                }
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user is null)
                {
                    user = new User
                    {
                        Id = userId,
                        IsValid = false,
                        ServerName = serverName,
                        SignupDate = DateTime.Now,
                        Username = userInfoJson.GetProperty("username").GetString(),
                        Role = "user"
                    };

                    _logger.LogInformation($"Adding new user [{JsonSerializer.Serialize<User>(user)}]");
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                await _auditRepo.PostAuditEvent(EventType.Authenticate, userId, null);

                var userCookie = new
                {
                    Id = user.Id.ToString(),
                    Username = user.ServerName,
                    Avatar = userInfoJson.GetProperty("avatar").GetString(),
                    IsValid = user.IsValid,
                    Role = user.Role
                };

                Response.Cookies.Append("auth", authContent, new CookieOptions { HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax, Secure = false, Expires = DateTime.Now.AddMinutes(30) });
                Response.Cookies.Append("user", JsonSerializer.Serialize(userCookie), new CookieOptions { HttpOnly = false, IsEssential = true, SameSite = SameSiteMode.Lax, Secure = false, Expires = DateTime.Now.AddMinutes(30) });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        return Ok();
    }
}