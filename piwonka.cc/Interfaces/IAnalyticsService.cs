using Piwonka.CC.Models;

namespace Piwonka.CC.Services
{
    public interface IAnalyticsService
    {
        Task TrackPageViewAsync(string sessionId, string? ipAddress, string? userAgent);
        Task<List<Analytics>> GetDailyStatsAsync(int days = 30);
        Task<Analytics?> GetTodayStatsAsync();
        Task ProcessDailyAnalyticsAsync();
    }
}
