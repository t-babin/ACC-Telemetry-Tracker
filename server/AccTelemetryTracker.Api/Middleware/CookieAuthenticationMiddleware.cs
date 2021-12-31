using System.Text.Json;
using AccTelemetryTracker.Datastore;

namespace AccTelemetryTracker.Api.Middleware
{
    public class CookieAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CookieAuthenticationMiddleware> _logger;
        private readonly IServiceScopeFactory _serviceProvider;

        public CookieAuthenticationMiddleware(RequestDelegate next, ILogger<CookieAuthenticationMiddleware> logger, IServiceScopeFactory serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path.Value.Equals("/auth/callback"))
            {
                await _next(context);
                return;
            }
            else if (context.Request.Cookies.Any(c => c.Key.Equals("user")) && context.Request.Cookies.Any(c => c.Key.Equals("auth")))
            {
                var authCookie = context.Request.Cookies.FirstOrDefault(c => c.Key.Equals("auth"));
                var userCookie = context.Request.Cookies.FirstOrDefault(c => c.Key.Equals("user"));
                if (!long.TryParse(JsonDocument.Parse(userCookie.Value).RootElement.GetProperty("Id").GetString(), out var userId))
                {
                    _logger.LogError("Error parsing the user cookie");
                    context.Items["valid"] = false;
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AccTelemetryTrackerContext>();
                    var user = await dbContext.Users.FindAsync(userId);
                    if (user == null)
                    {
                        _logger.LogError($"User with ID [{userId.ToString()}] doesn't exist");
                        context.Items["valid"] = false;
                    }
                    else
                    {
                        context.Items["valid"] = true;
                    }
                }

            }
            else
            {
                _logger.LogInformation("Request does not contain both the auth and user cookies");
                context.Items["valid"] = false;
            }
            await _next(context);
        }
    }
}