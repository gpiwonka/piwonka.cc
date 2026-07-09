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
        private readonly Piwonka.CC.Models.SiteSettings _site;

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

        // Datei für persistenten Fallback-Key, wenn Config keinen Wert liefert.
        private static readonly string KeyCachePath = Path.Combine(AppContext.BaseDirectory, "indexnow.key");
        private static readonly object KeyCacheLock = new();

        public IndexNowService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<IndexNowService> logger,
            Piwonka.CC.Models.SiteSettings site)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _site = site;

            // Konfiguration lesen — leere Strings wie null behandeln (Env-Vars / Production-Overrides liefern manchmal "" statt nichts).
            var configKey = _configuration["IndexNow:Key"];
            if (string.IsNullOrWhiteSpace(configKey))
            {
                _key = LoadOrCreatePersistedKey();
                _logger.LogWarning("IndexNow:Key fehlt in Konfiguration — verwende persistierten Fallback-Key {KeyFile}", KeyCachePath);
            }
            else
            {
                _key = configKey.Trim();
            }

            _host = _configuration["IndexNow:Host"] ?? _site.Name;
            _keyLocation = $"https://{_host}/{_key}.txt";
            _isEnabled = _configuration.GetValue<bool>("IndexNow:Enabled", true);

            _logger.LogInformation("IndexNow Service initialized. Enabled: {Enabled}, Host: {Host}, KeyLength: {KeyLength}", _isEnabled, _host, _key.Length);
        }

        private string LoadOrCreatePersistedKey()
        {
            lock (KeyCacheLock)
            {
                try
                {
                    if (File.Exists(KeyCachePath))
                    {
                        var existing = File.ReadAllText(KeyCachePath).Trim();
                        if (!string.IsNullOrWhiteSpace(existing)) return existing;
                    }

                    var newKey = GenerateApiKey();
                    File.WriteAllText(KeyCachePath, newKey);
                    return newKey;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Konnte persistenten IndexNow-Key nicht lesen/schreiben — generiere flüchtigen Key");
                    return GenerateApiKey();
                }
            }
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
            return Guid.NewGuid().ToString();
        }

        // Methode um den API Key für die Key-Datei zu erhalten
        public string GetApiKey() => _key;
    }
}
