using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using System.Security.Cryptography;
using System.Text;

namespace Piwonka.CC.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<AnalyticsService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task TrackPageViewAsync(string sessionId, string? ipAddress, string? userAgent)
        {
            try
            {
                using var _context = await _contextFactory.CreateDbContextAsync();      
                var today = DateOnly.FromDateTime(DateTime.Now);

                // Prüfen ob Session heute schon existiert
                var existingSession = await _context.UserSessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.Date == today);

                if (existingSession == null)
                {
                    // Neue Session erstellen
                    var newSession = new UserSession
                    {
                        SessionId = sessionId,
                        IpAddress = HashIpAddress(ipAddress),
                        UserAgent = userAgent?.Substring(0, Math.Min(500, userAgent.Length)),
                        Date = today,
                        PageViewCount = 1
                    };

                    _context.UserSessions.Add(newSession);
                }
                else
                {
                    // PageView zählen und letzte Aktivität updaten
                    existingSession.PageViewCount++;
                    existingSession.LastActivity = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Tracking der Page View");
            }
        }

        public async Task<List<Analytics>> GetDailyStatsAsync(int days = 30)
        {
            var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-days));
            using var _context = await _contextFactory.CreateDbContextAsync();
            return await _context.Analytics
                .Where(a => a.Date >= startDate)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<Analytics?> GetTodayStatsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            using var _context = await _contextFactory.CreateDbContextAsync();

            var todaySessions = await _context.UserSessions
                .Where(s => s.Date == today)
                .ToListAsync();

            if (!todaySessions.Any())
                return null;

            return new Analytics
            {
                Date = today,
                UniqueVisitors = todaySessions.Count,
                PageViews = todaySessions.Sum(s => s.PageViewCount)
            };
        }

        public async Task ProcessDailyAnalyticsAsync()
        {
            try
            {
                using var _context = await _contextFactory.CreateDbContextAsync();
                var yesterday = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

                // Prüfen ob bereits verarbeitet
                var existing = await _context.Analytics
                    .FirstOrDefaultAsync(a => a.Date == yesterday);

                if (existing != null)
                {
                    _logger.LogInformation($"Analytics für {yesterday} bereits vorhanden");
                    return;
                }

                // Sessions von gestern zählen
                var uniqueVisitors = await _context.UserSessions
                    .Where(s => s.Date == yesterday)
                    .CountAsync();

                // Page Views aus den Sessions summieren
                var pageViews = await _context.UserSessions
                    .Where(s => s.Date == yesterday)
                    .SumAsync(s => s.PageViewCount);

                if (uniqueVisitors > 0)
                {
                    var analytics = new Analytics
                    {
                        Date = yesterday,
                        UniqueVisitors = uniqueVisitors,
                        PageViews = pageViews
                    };

                    _context.Analytics.Add(analytics);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Analytics verarbeitet: {yesterday} - {uniqueVisitors} Unique Visitors");
                }

                // Alte Sessions löschen (älter als 7 Tage)
                var cutoffDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
                var oldSessions = await _context.UserSessions
                    .Where(s => s.Date < cutoffDate)
                    .ToListAsync();

                if (oldSessions.Any())
                {
                    _context.UserSessions.RemoveRange(oldSessions);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"{oldSessions.Count} alte Sessions gelöscht");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei der Verarbeitung der tägl. Analytics");
            }
        }

        private string? HashIpAddress(string? ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return null;

            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(ipAddress + "salt_key"));
            return Convert.ToBase64String(hashedBytes)[..10]; // Nur ersten 10 Zeichen
        }
    }
}
