using Piwonka.CC.Services;

namespace Piwonka.CC.Jobs
{
    public class AnalyticsJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AnalyticsJob> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Jede Stunde prüfen

        public AnalyticsJob(IServiceProvider serviceProvider, ILogger<AnalyticsJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

                    await analyticsService.ProcessDailyAnalyticsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler im Analytics Background Job");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}