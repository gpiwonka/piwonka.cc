using Microsoft.AspNetCore.Http;
using Piwonka.CC.Services;
using System.Threading.Tasks;

namespace Piwonka.CC.Middleware
{
    public class SitemapUpdateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SitemapUpdateMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // Prüfen ob es sich um eine Admin-Aktion handelt, die die Sitemap beeinflussen könnte
            var path = context.Request.Path.Value?.ToLowerInvariant();
            var method = context.Request.Method;

            if (method == "POST" && path != null &&
                (path.Contains("/admin/seiten/") || path.Contains("/admin/blog/")) &&
                context.Response.StatusCode < 400)
            {
                // Fire-and-forget Sitemap-Update
                _ = Task.Run(async () =>
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var indexNowService = scope.ServiceProvider.GetRequiredService<IIndexNowService>();

                    try
                    {
                        await Task.Delay(5000); // 5 Sekunden warten, damit DB-Änderungen abgeschlossen sind
                        await indexNowService.NotifySitemapUpdatedAsync();
                    }
                    catch
                    {
                        // Fehler ignorieren, da das nur ein Best-Effort-Update ist
                    }
                });
            }
        }
    }
    public static class SitemapUpdateMiddlewareExtensions
    {
        public static IApplicationBuilder UseSitemapUpdateNotification(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SitemapUpdateMiddleware>();
        }
    }
}