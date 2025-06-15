using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Pages.Admin.Analytics
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly IAnalyticsService _analyticsService;

        public IndexModel(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public List<Models.Analytics> DailyStats { get; set; } = new();
        public Models.Analytics? TodayStats { get; set; }
        public AnalyticsChartData ChartData { get; set; } = new();

        public async Task OnGetAsync()
        {
            DailyStats = await _analyticsService.GetDailyStatsAsync(30);
            TodayStats = await _analyticsService.GetTodayStatsAsync();

            // Chart Daten vorbereiten
            ChartData.Labels = DailyStats
                .OrderBy(d => d.Date)
                .Select(d => d.Date.ToString("dd.MM"))
                .ToList();

            ChartData.Visitors = DailyStats
                .OrderBy(d => d.Date)
                .Select(d => d.UniqueVisitors)
                .ToList();

            ChartData.PageViews = DailyStats
                .OrderBy(d => d.Date)
                .Select(d => d.PageViews)
                .ToList();
        }
    }

    public class AnalyticsChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Visitors { get; set; } = new();
        public List<int> PageViews { get; set; } = new();
    }
}