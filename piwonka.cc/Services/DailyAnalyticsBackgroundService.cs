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

        public DailyAnalyticsBackgroundService(
            ILogger<DailyAnalyticsBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Berechne Zeit bis zur nächsten Ausführung (täglich um 2:00 Uhr)
                var now = DateTime.Now;
                DateTime nextRun;

                if (now.Hour < 2)
                {
                    // Noch vor 2 Uhr heute -> heute um 2:00 ausführen
                    nextRun = DateTime.Today.AddHours(2);
                }
                else
                {
                    // Schon nach 2 Uhr -> morgen um 2:00 ausführen
                    nextRun = DateTime.Today.AddDays(1).AddHours(2);
                }

                var delay = nextRun - now;
                _logger.LogInformation("Daily Analytics Service: Nächste Ausführung um {NextRun}", nextRun);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                await ProcessAnalytics();
            }
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

    }
}