// Middleware/AnalyticsMiddleware.cs
using Piwonka.CC.Services;
using System.Text;

namespace Piwonka.CC.Middleware
{
    public class AnalyticsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AnalyticsMiddleware> _logger;

        public AnalyticsMiddleware(RequestDelegate next, ILogger<AnalyticsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAnalyticsService analyticsService)
        {
            // Page Views nur für GET-Requests und normale Seiten tracken
            if (context.Request.Method == "GET" &&
                !context.Request.Path.StartsWithSegments("/Admin") &&
                !context.Request.Path.StartsWithSegments("/css") &&
                !context.Request.Path.StartsWithSegments("/js") &&
                !context.Request.Path.StartsWithSegments("/lib") &&
                !context.Request.Path.StartsWithSegments("/images") &&
                !context.Request.Path.StartsWithSegments("/uploads"))
            {
                try
                {
                    var sessionId = GetOrCreateSessionId(context);
                    var ipAddress = GetClientIpAddress(context);
                    var userAgent = context.Request.Headers["User-Agent"];

                    // Async tracking (nicht warten)
                    _ = Task.Run(async () =>
                    {
                        await analyticsService.TrackPageViewAsync(sessionId, ipAddress, userAgent);
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Analytics Tracking Fehler");
                }
            }

            await _next(context);
        }

        private string GetOrCreateSessionId(HttpContext context)
        {
            const string sessionKey = "AnalyticsSessionId";

            if (context.Session.TryGetValue(sessionKey, out var sessionBytes))
            {
                return Encoding.UTF8.GetString(sessionBytes);
            }

            var newSessionId = Guid.NewGuid().ToString();
            context.Session.Set(sessionKey, Encoding.UTF8.GetBytes(newSessionId));
            return newSessionId;
        }

        private string? GetClientIpAddress(HttpContext context)
        {
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.ToString().Split(',')[0].Trim();
            }

            var xRealIp = context.Request.Headers["X-Real-IP"];
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp.ToString();
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}