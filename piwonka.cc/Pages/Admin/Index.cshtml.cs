// Pages/Admin/Index.cshtml.cs - Erweitert mit Analytics
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Filters;
using Piwonka.CC.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages.Admin
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IWebHostEnvironment _environment;
        private readonly IAnalyticsService _analyticsService;

        public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory, IWebHostEnvironment environment, IAnalyticsService analyticsService)
        {
            _contextFactory = contextFactory;
            _environment = environment;
            _analyticsService = analyticsService;
        }

        public DashboardStatistics Statistics { get; set; } = new();
        public AnalyticsSummary Analytics { get; set; } = new();
        public List<Seite> RecentSeiten { get; set; } = new();
        public List<Post> RecentPosts { get; set; } = new();
        public string Environment => _environment.EnvironmentName;

        public async Task OnGetAsync()
        {
         using var _context = await _contextFactory.CreateDbContextAsync();
            // Debug-Informationen
            Console.WriteLine($"=== ADMIN DASHBOARD ===");
            Console.WriteLine($"Aktuelle Umgebung: {_environment.EnvironmentName}");
            // Sicherstellen, dass die Datenbank initialisiert ist
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

        private string CalculateTrend(List<Models.Analytics> analytics)
        {
            if (analytics.Count < 2) return "neutral";

            var recent = analytics.Take(3).Sum(a => a.UniqueVisitors);
            var older = analytics.Skip(3).Take(4).Sum(a => a.UniqueVisitors);

            if (recent > older) return "up";
            if (recent < older) return "down";
            return "neutral";
        }
    }

    public class DashboardStatistics
    {
        public int TotalSeiten { get; set; }
        public int VeroeffentlichteSeiten { get; set; }
        public int TotalPosts { get; set; }
        public int VeroeffentlichtePosts { get; set; }
        public int TotalKategorien { get; set; }
        public int EntwuerfeCount { get; set; }
    }

    public class AnalyticsSummary
    {
        public int TodayVisitors { get; set; }
        public int TodayPageViews { get; set; }
        public int Last7DaysVisitors { get; set; }
        public int Last7DaysPageViews { get; set; }
        public string TrendDirection { get; set; } = "neutral"; // "up", "down", "neutral"
    }
}