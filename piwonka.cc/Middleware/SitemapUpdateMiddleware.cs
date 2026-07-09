using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Middleware
{
    public class SitemapUpdateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SitemapUpdateMiddleware> _logger;
        private readonly SiteSettings _site;

        public SitemapUpdateMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory, ILogger<SitemapUpdateMiddleware> logger, SiteSettings site)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _site = site;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // Nur bei erfolgreichen POST-Requests auf Admin-Seiten
            if (context.Request.Method != "POST" || context.Response.StatusCode >= 400)
                return;

            var path = context.Request.Path.Value?.ToLowerInvariant();
            if (path == null || !path.StartsWith("/admin/"))
                return;

            // Welche Content-URLs wurden geändert?
            List<string>? urlsToNotify = null;

            if (path.StartsWith("/admin/posts/") || path.StartsWith("/admin/blog/"))
            {
                urlsToNotify = await GetPublishedPostUrlsAsync();
            }
            else if (path.StartsWith("/admin/seiten/"))
            {
                urlsToNotify = await GetPublishedSeiteUrlsAsync();
            }

            if (urlsToNotify == null || urlsToNotify.Count == 0)
                return;

            // Sitemap immer mit melden
            urlsToNotify.Add($"{_site.Url}/sitemap.xml");

            _ = Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var indexNowService = scope.ServiceProvider.GetRequiredService<IIndexNowService>();

                try
                {
                    await Task.Delay(3000); // Kurz warten bis DB-Änderungen committed sind
                    await indexNowService.NotifyUrlsAsync(urlsToNotify);
                    _logger.LogInformation("IndexNow: {Count} URLs gesendet nach Admin-Aktion auf {Path}", urlsToNotify.Count, path);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "IndexNow-Benachrichtigung fehlgeschlagen für {Path}", path);
                }
            });
        }

        private async Task<List<string>> GetPublishedPostUrlsAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var context = await contextFactory.CreateDbContextAsync();

            var slugs = await context.Posts
                .Where(p => p.IstVeroeffentlicht)
                .Select(p => p.Slug)
                .ToListAsync();
            return slugs.Select(slug => $"{_site.Url}/blog/{slug}").ToList();
        }

        private async Task<List<string>> GetPublishedSeiteUrlsAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var context = await contextFactory.CreateDbContextAsync();

            var slugs = await context.Seiten
                .Where(s => s.IstVeroeffentlicht)
                .Select(s => s.Slug)
                .ToListAsync();
            return slugs.Select(slug => $"{_site.Url}/seite/{slug}").ToList();
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
