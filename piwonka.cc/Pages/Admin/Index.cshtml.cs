// Pages/Admin/Index.cshtml.cs - Vollständige korrigierte Version
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Filters;
using Piwonka.CC.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Piwonka.CC.Pages.Admin
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IWebHostEnvironment _environment;
        private readonly IAnalyticsService _analyticsService;
        private readonly IIndexNowService _indexNowService;
        private readonly IConfiguration _configuration;

        public IndexModel(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IWebHostEnvironment environment,
            IAnalyticsService analyticsService,
            IIndexNowService indexNowService,
            IConfiguration configuration)
        {
            _contextFactory = contextFactory;
            _environment = environment;
            _analyticsService = analyticsService;
            _indexNowService = indexNowService;
            _configuration = configuration;
        }

        public DashboardStatistics Statistics { get; set; } = new();
        public AnalyticsSummary Analytics { get; set; } = new();
        public IndexNowSummary IndexNowStats { get; set; } = new();
        public List<Seite> RecentSeiten { get; set; } = new();
        public List<Post> RecentPosts { get; set; } = new();
        public string Environment => _environment.EnvironmentName;

        public async Task OnGetAsync()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            Console.WriteLine($"=== ADMIN DASHBOARD ===");
            Console.WriteLine($"Aktuelle Umgebung: {_environment.EnvironmentName}");

            await _context.Database.EnsureCreatedAsync();

            // Statistiken laden
            Statistics = new DashboardStatistics
            {
                TotalSeiten = await _context.Seiten.CountAsync(),
                VeroeffentlichteSeiten = await _context.Seiten.CountAsync(s => s.IstVeroeffentlicht),
                TotalPosts = await _context.Posts.CountAsync(),
                VeroeffentlichtePosts = await _context.Posts.CountAsync(b => b.IstVeroeffentlicht),
                TotalKategorien = await _context.Kategorien.CountAsync(),
                EntwuerfeCount = await _context.Seiten.CountAsync(s => !s.IstVeroeffentlicht) +
                                await _context.Posts.CountAsync(b => !b.IstVeroeffentlicht)
            };

            // Analytics laden
            try
            {
                var todayStats = await _analyticsService.GetTodayStatsAsync();
                var last7Days = await _analyticsService.GetDailyStatsAsync(7);

                Analytics = new AnalyticsSummary
                {
                    TodayVisitors = todayStats?.UniqueVisitors ?? 0,
                    TodayPageViews = todayStats?.PageViews ?? 0,
                    Last7DaysVisitors = last7Days.Sum(a => a.UniqueVisitors),
                    Last7DaysPageViews = last7Days.Sum(a => a.PageViews),
                    TrendDirection = CalculateTrend(last7Days)
                };
            }
            catch (Exception)
            {
                // Fallback wenn Analytics Service nicht verfügbar
                Analytics = new AnalyticsSummary
                {
                    TodayVisitors = 0,
                    TodayPageViews = 0,
                    Last7DaysVisitors = 0,
                    Last7DaysPageViews = 0,
                    TrendDirection = "neutral"
                };
            }

            // IndexNow Statistiken laden
            try
            {
                IndexNowStats = new IndexNowSummary
                {
                    IsEnabled = _configuration.GetValue<bool>("IndexNow:Enabled", true),
                    Host = _configuration["IndexNow:Host"] ?? "piwonka.cc",
                    TotalIndexableUrls = Statistics.VeroeffentlichteSeiten + Statistics.VeroeffentlichtePosts,
                    LastUpdateTime = DateTime.Now,
                    ApiKey = GetIndexNowApiKey()
                };
            }
            catch (Exception)
            {
                // Fallback wenn IndexNow Service nicht verfügbar
                IndexNowStats = new IndexNowSummary
                {
                    IsEnabled = false,
                    Host = "piwonka.cc",
                    TotalIndexableUrls = Statistics.VeroeffentlichteSeiten + Statistics.VeroeffentlichtePosts,
                    LastUpdateTime = DateTime.Now,
                    ApiKey = "N/A"
                };
            }

            // Letzte Seiten (Top 5)
            RecentSeiten = await _context.Seiten
                .OrderByDescending(s => s.ErstelltAm)
                .Take(5)
                .ToListAsync();

            // Letzte Blog-Posts (Top 5)
            RecentPosts = await _context.Posts
                .OrderByDescending(b => b.ErstelltAm)
                .Take(5)
                .ToListAsync();
        }

        // IndexNow Quick Actions
        public async Task<IActionResult> OnPostQuickNotifyAllAsync()
        {
            try
            {
                using var _context = await _contextFactory.CreateDbContextAsync();

                var publishedPages = await _context.Seiten
                    .Where(s => s.IstVeroeffentlicht)
                    .Select(s => s.Slug)
                    .ToListAsync();

                var publishedPosts = await _context.Posts
                    .Where(p => p.IstVeroeffentlicht)
                    .Select(p => p.Slug)
                    .ToListAsync();

                var urls = new List<string>();
                urls.AddRange(publishedPages.Select(slug => $"/seite/{slug}"));
                urls.AddRange(publishedPosts.Select(slug => $"/blog/{slug}"));

                if (urls.Any())
                {
                    // Fire-and-forget für bessere Performance
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _indexNowService.NotifyUrlsAsync(urls);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"IndexNow bulk notification failed: {ex.Message}");
                        }
                    });

                    TempData["SuccessMessage"] = $"IndexNow-Benachrichtigung für {urls.Count} URLs gestartet.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Keine veröffentlichten Inhalte zum Benachrichtigen gefunden.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Fehler bei IndexNow-Benachrichtigung: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostQuickNotifySitemapAsync()
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _indexNowService.NotifySitemapUpdatedAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"IndexNow sitemap notification failed: {ex.Message}");
                    }
                });

                TempData["SuccessMessage"] = "Sitemap-Benachrichtigung an IndexNow gesendet.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Fehler bei Sitemap-Benachrichtigung: {ex.Message}";
            }

            return RedirectToPage();
        }

        private string CalculateTrend(List<Models.Analytics> analytics)
        {
            if (analytics.Count < 2) return "neutral";

            var recent = analytics.Take(3).Sum(a => a.UniqueVisitors);
            var older = analytics.Skip(3).Take(4).Sum(a => a.UniqueVisitors);

            if (recent > older) return "up";
            if (recent < older) return "down";
            return "neutral";
        }

        private string GetIndexNowApiKey()
        {
            try
            {
                if (_indexNowService is IndexNowService indexNowService)
                {
                    var fullKey = indexNowService.GetApiKey();
                    return fullKey.Length > 8 ? fullKey.Substring(0, 8) + "..." : fullKey;
                }
            }
            catch
            {
                // Ignorieren wenn Service nicht verfügbar
            }
            return "N/A";
        }
    }

    // Dashboard Statistiken
    public class DashboardStatistics
    {
        public int TotalSeiten { get; set; }
        public int VeroeffentlichteSeiten { get; set; }
        public int TotalPosts { get; set; }
        public int VeroeffentlichtePosts { get; set; }
        public int TotalKategorien { get; set; }
        public int EntwuerfeCount { get; set; }
    }

    // Analytics Zusammenfassung
    public class AnalyticsSummary
    {
        public int TodayVisitors { get; set; }
        public int TodayPageViews { get; set; }
        public int Last7DaysVisitors { get; set; }
        public int Last7DaysPageViews { get; set; }
        public string TrendDirection { get; set; } = "neutral"; // "up", "down", "neutral"
    }

    // IndexNow Zusammenfassung - DIESE KLASSE FEHLTE!
    public class IndexNowSummary
    {
        public bool IsEnabled { get; set; }
        public string Host { get; set; } = string.Empty;
        public int TotalIndexableUrls { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string ApiKey { get; set; } = string.Empty;

        // Computed Properties für die View
        public string StatusText => IsEnabled ? "Aktiv" : "Deaktiviert";
        public string StatusColor => IsEnabled ? "success" : "secondary";
    }
}
