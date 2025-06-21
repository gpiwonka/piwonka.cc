using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Piwonka.CC.Services
{
    public class IndexNowService : IIndexNowService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IndexNowService> _logger;

        private readonly string _key;
        private readonly string _keyLocation;
        private readonly string _host;
        private readonly bool _isEnabled;

        // IndexNow Endpoints
        private readonly List<string> _indexNowEndpoints = new()
        {
            "https://api.indexnow.org/indexnow",
            "https://www.bing.com/indexnow",
            "https://yandex.com/indexnow"
        };

        public IndexNowService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<IndexNowService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Konfiguration lesen
            _key = _configuration["IndexNow:Key"] ?? GenerateApiKey();
            _host = _configuration["IndexNow:Host"] ?? "piwonka.cc";
            _keyLocation = $"https://{_host}/{_key}.txt";
            _isEnabled = _configuration.GetValue<bool>("IndexNow:Enabled", true);

            _logger.LogInformation($"IndexNow Service initialized. Enabled: {_isEnabled}, Host: {_host}");
        }

        public async Task NotifyUrlAsync(string url)
        {
            if (!_isEnabled)
            {
                _logger.LogDebug("IndexNow is disabled, skipping notification for {Url}", url);
                return;
            }

            await NotifyUrlsAsync(new[] { url });
        }

        public async Task NotifyUrlsAsync(IEnumerable<string> urls)
        {
            if (!_isEnabled)
            {
                _logger.LogDebug("IndexNow is disabled, skipping notification for {Count} URLs", urls.Count());
                return;
            }

            var urlList = urls.ToList();
            if (!urlList.Any())
            {
                _logger.LogDebug("No URLs to notify");
                return;
            }

            // URLs validieren und vollständig machen
            var validUrls = urlList
                .Where(url => !string.IsNullOrEmpty(url))
                .Select(url => url.StartsWith("http") ? url : $"https://{_host}{(url.StartsWith("/") ? url : "/" + url)}")
                .ToList();

            if (!validUrls.Any())
            {
                _logger.LogWarning("No valid URLs found to notify");
                return;
            }

            var payload = new
            {
                host = _host,
                key = _key,
                keyLocation = _keyLocation,
                urlList = validUrls
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogInformation("Notifying IndexNow about {Count} URLs: {Urls}",
                validUrls.Count, string.Join(", ", validUrls));

            // Parallel zu allen Endpunkten senden
            var tasks = _indexNowEndpoints.Select(endpoint => SendNotificationAsync(endpoint, json));
            var results = await Task.WhenAll(tasks);

            var successCount = results.Count(r => r);
            _logger.LogInformation("IndexNow notification completed. {Success}/{Total} endpoints succeeded",
                successCount, _indexNowEndpoints.Count);
        }

        public async Task NotifyPostCreatedAsync(string slug)
        {
            var url = $"https://{_host}/blog/{slug}";
            await NotifyUrlAsync(url);
            _logger.LogInformation("Notified IndexNow about new blog post: {Url}", url);
        }

        public async Task NotifyPostUpdatedAsync(string slug)
        {
            var url = $"https://{_host}/blog/{slug}";
            await NotifyUrlAsync(url);
            _logger.LogInformation("Notified IndexNow about updated blog post: {Url}", url);
        }

        public async Task NotifySeiteCreatedAsync(string slug)
        {
            var url = $"https://{_host}/seite/{slug}";
            await NotifyUrlAsync(url);
            _logger.LogInformation("Notified IndexNow about new page: {Url}", url);
        }

        public async Task NotifySeiteUpdatedAsync(string slug)
        {
            var url = $"https://{_host}/seite/{slug}";
            await NotifyUrlAsync(url);
            _logger.LogInformation("Notified IndexNow about updated page: {Url}", url);
        }

        public async Task NotifySitemapUpdatedAsync()
        {
            var sitemapUrl = $"https://{_host}/sitemap.xml";
            await NotifyUrlAsync(sitemapUrl);
            _logger.LogInformation("Notified IndexNow about updated sitemap: {Url}", sitemapUrl);
        }

        private async Task<bool> SendNotificationAsync(string endpoint, string json)
        {
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Successfully sent IndexNow notification to {Endpoint}", endpoint);
                    return true;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("IndexNow notification failed for {Endpoint}. Status: {Status}, Response: {Response}",
                        endpoint, response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending IndexNow notification to {Endpoint}", endpoint);
                return false;
            }
        }

        private static string GenerateApiKey()
        {
            // Generiert einen zufälligen 32-stelligen Hex-String als API Key
            var random = new Random();
            var bytes = new byte[16];
            random.NextBytes(bytes);
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        // Methode um den API Key für die Key-Datei zu erhalten
        public string GetApiKey() => _key;
    }
}
