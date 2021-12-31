using AccTelemetryTracker.Datastore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AccTelemetryTracker.Api.Attributes
{
    public class CookieAuthorizeFilter : IAsyncResourceFilter
    {
        private readonly ILogger<CookieAuthorizeFilter> _logger;
        private readonly AccTelemetryTrackerContext _context;

        public CookieAuthorizeFilter(ILogger<CookieAuthorizeFilter> logger, AccTelemetryTrackerContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (context.HttpContext.Items.TryGetValue("valid", out var valid))
            {
                var isValid = (bool) valid!;
                if (!isValid)
                {
                    _logger.LogError("Incoming request was invalid - cookies were not provided");
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    await next();
                }
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}