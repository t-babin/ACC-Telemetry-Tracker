using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AccTelemetryTracker.Logic;

public class DiscordNotifier : IDiscordNotifier
{
    private readonly IConfiguration _config;
    private readonly ILogger<DiscordNotifier> _logger;
    public DiscordNotifier(IConfiguration config, ILogger<DiscordNotifier> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Notify(Datastore.Models.MotecFile motecFile, string? avatarUrl)
    {
        var hookUrl = _config.GetValue<string>("DISCORD_WEBHOOK_URL");
        if (string.IsNullOrEmpty(hookUrl))
        {
            _logger.LogInformation("No webhook URL set");
            return;
        }

        try
        {
            var minutes = Math.Floor(motecFile.FastestLap / 60);
            // 113 - 60
            var seconds = Math.Floor(motecFile.FastestLap - (minutes * 60));
            // 0.849
            var milliseconds = Math.Round(((motecFile.FastestLap - (minutes * 60) - seconds) * 1000), 0);

            var fields = new List<Fields> {
                new Fields
                {
                    name = "Track",
                    value = motecFile.Track.Name,
                    inline = true
                },
                new Fields 
                {
                    name = "Car",
                    value = motecFile.Car.Name,
                    inline = true
                },
                new Fields
                {
                    name = "Fastest Lap",
                    value = $"{minutes:00}:{seconds:00}.{milliseconds:000}",
                    inline = true
                },
                new Fields
                {
                    name = "Track Conditions",
                    value = motecFile.TrackCondition.ToString(),
                    inline = false
                }
            };
            if (!string.IsNullOrEmpty(motecFile.Comment))
            {
                fields.Add(new Fields
                {
                    name = "Comment",
                    value = motecFile.Comment,
                    inline = false
                });
            }

            var postObject = new
            {
                username = "ACC Telemetry Tracker",
                embeds = new object[] {
                        new {
                            author = new {
                                name = $"{motecFile.User.ServerName} has uploaded a new file.",
                                icon_url = string.IsNullOrEmpty(avatarUrl) ? "" : $"https://cdn.discordapp.com/avatars/{motecFile.UserId}/{avatarUrl}.png"
                            },
                            color = 4151711,
                            fields = fields
                        }
                    }
            };
            var postBody = JsonSerializer.Serialize(postObject);
            _logger.LogInformation($"Sending data to webhook: {{{postBody}}}");
            var postContent = new StringContent(postBody, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            var response = await client.PostAsync(hookUrl, postContent);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occurred when sending Discord webhook");
            _logger.LogError(ex, ex.Message);
        }
    }
}

public class Fields
{
    public string? name { get; set; }
    
    public string? value { get; set; }
    
    public bool inline { get; set; }
}