using Microsoft.Extensions.Options;
using Piwonka.CC.Models;
using System.Text;
using System.Text.Json;

namespace Piwonka.CC.Services
{
    public class DbaToolService
    {
        private readonly HttpClient _httpClient;
        private readonly AnthropicSettings _settings;
        private readonly ILogger<DbaToolService> _logger;

        public DbaToolService(HttpClient httpClient, IOptions<AnthropicSettings> settings, ILogger<DbaToolService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public Task<string> FormatQueryAsync(string sql, string language)
        {
            var systemPrompt = language == "de"
                ? "Du bist ein SQL Server Experte. Formatiere die folgende SQL-Abfrage sauber und übersichtlich. Erkläre anschließend kurz, was die Abfrage macht. Verwende Markdown: den formatierten SQL-Code in einem ```sql Block, die Erklärung darunter."
                : "You are a SQL Server expert. Format the following SQL query cleanly and clearly. Then briefly explain what the query does. Use Markdown: the formatted SQL in a ```sql block, the explanation below.";

            return CallAnthropicAsync(systemPrompt, sql);
        }

        public Task<string> AnalyzeIndexesAsync(string ddlAndQueries, string language)
        {
            var systemPrompt = language == "de"
                ? "Du bist ein erfahrener SQL Server DBA und Index-Experte. Analysiere die folgenden Tabellendefinitionen (DDL) und Queries. Gib konkrete Index-Empfehlungen: 1) Welche Indizes erstellt werden sollten (mit CREATE INDEX Statements), 2) Warum jeder Index hilft, 3) Welche bestehenden Indizes ggf. überflüssig sind, 4) Auswirkung auf Write-Performance. Formatiere mit Markdown."
                : "You are an experienced SQL Server DBA and indexing expert. Analyze the following table definitions (DDL) and queries. Provide concrete index recommendations: 1) Which indexes should be created (with CREATE INDEX statements), 2) Why each index helps, 3) Which existing indexes might be redundant, 4) Impact on write performance. Format with Markdown.";

            return CallAnthropicAsync(systemPrompt, ddlAndQueries);
        }

        public Task<string> AnalyzeDeadlockAsync(string deadlockXml, string language)
        {
            var systemPrompt = language == "de"
                ? "Du bist ein SQL Server Experte für Concurrency und Locking. Analysiere den folgenden Deadlock-Graph (XML aus SQL Server). Erkläre: 1) Welche Prozesse beteiligt waren, 2) Welche Ressourcen/Locks den Deadlock verursacht haben, 3) Welcher Prozess als Opfer gewählt wurde und warum, 4) Konkrete Lösungsvorschläge um den Deadlock zu vermeiden (mit Code-Beispielen). Formatiere mit Markdown."
                : "You are a SQL Server concurrency and locking expert. Analyze the following deadlock graph (XML from SQL Server). Explain: 1) Which processes were involved, 2) Which resources/locks caused the deadlock, 3) Which process was chosen as victim and why, 4) Concrete solutions to avoid the deadlock (with code examples). Format with Markdown.";

            return CallAnthropicAsync(systemPrompt, deadlockXml);
        }

        public Task<string> ConvertQueryAsync(string sql, string sourceDialect, string targetDialect, string language)
        {
            var systemPrompt = language == "de"
                ? $"Du bist ein SQL-Experte für mehrere Datenbanksysteme. Konvertiere die folgende SQL-Abfrage von {sourceDialect} nach {targetDialect}. Zeige: 1) Die konvertierte Abfrage in einem ```sql Block, 2) Eine Erklärung aller Änderungen, 3) Wichtige Unterschiede und Fallstricke zwischen den beiden Dialekten für diese Abfrage. Formatiere mit Markdown."
                : $"You are a SQL expert for multiple database systems. Convert the following SQL query from {sourceDialect} to {targetDialect}. Show: 1) The converted query in a ```sql block, 2) An explanation of all changes, 3) Important differences and pitfalls between the two dialects for this query. Format with Markdown.";

            return CallAnthropicAsync(systemPrompt, sql);
        }

        private async Task<string> CallAnthropicAsync(string systemPrompt, string userInput)
        {
            var requestBody = new
            {
                model = _settings.Model,
                max_tokens = 4096,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userInput }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("x-api-key", _settings.ApiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Anthropic API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
                throw new Exception($"API-Fehler: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(responseContent);
            var textContent = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            return textContent ?? "Keine Antwort erhalten.";
        }
    }
}
