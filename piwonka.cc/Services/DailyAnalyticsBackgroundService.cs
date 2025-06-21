// Services/DailyAnalyticsBackgroundService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Piwonka.CC.Services;

namespace Piwonka.CC.Services
{
    public class DailyAnalyticsBackgroundService : BackgroundService
    {
        private readonly ILogger<DailyAnalyticsBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public DailyAnalyticsBackgroundService(
            ILogger<DailyAnalyticsBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Berechne Zeit bis zur nächsten Ausführung (täglich um 2:00 Uhr)
            var now = DateTime.Now;
            var nextRun = DateTime.Today.AddDays(1).AddHours(2); // Morgen um 2:00 Uhr

            // Falls es schon nach 2:00 Uhr heute ist, führe heute noch aus
            if (now.Hour >= 2)
            {
                nextRun = DateTime.Today.AddHours(2);
            }

            var initialDelay = nextRun - now;

            _logger.LogInformation("Daily Analytics Service startet. Nächste Ausführung: {NextRun}", nextRun);

            // Warte bis zur ersten Ausführung
            if (initialDelay.TotalMilliseconds > 0)
            {
                await Task.Delay(initialDelay, stoppingToken);
            }

            ProcessAnalytics();

            // Erstelle Timer für tägliche Ausführung (alle 24 Stunden)
            _timer = new Timer(async _ => await ProcessAnalytics(), null, TimeSpan.Zero, TimeSpan.FromHours(24));

            // Halte den Service am Leben
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessAnalytics()
        {
            try
            {
                _logger.LogInformation("Starte tägliche Analytics-Verarbeitung um {Time}", DateTime.Now);

                using var scope = _serviceProvider.CreateScope();
                var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

                await analyticsService.ProcessDailyAnalyticsAsync();

                _logger.LogInformation("Tägliche Analytics-Verarbeitung erfolgreich abgeschlossen um {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei der täglichen Analytics-Verarbeitung: {Error}", ex.Message);
            }
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}