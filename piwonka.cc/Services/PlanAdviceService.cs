using Microsoft.Extensions.Options;
using Piwonka.CC.Models;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace Piwonka.CC.Services
{
    public class PlanAdviceService
    {
        private readonly HttpClient _httpClient;
        private readonly AnthropicSettings _settings;
        private readonly ILogger<PlanAdviceService> _logger;

        public const int MaxDirectChars = 100_000;

        public PlanAdviceService(HttpClient httpClient, IOptions<AnthropicSettings> settings, ILogger<PlanAdviceService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<(string Analysis, bool WasTruncated)> AnalyzeExecutionPlanAsync(string executionPlan, string language)
        {
            bool wasTruncated = false;
            string planToAnalyze = executionPlan;

            if (executionPlan.Length > MaxDirectChars)
            {
                var extracted = TryExtractExpensiveOperations(executionPlan, language);
                if (extracted != null)
                {
                    planToAnalyze = extracted;
                    wasTruncated = true;
                }
                else
                {
                    // Kein XML — Text einfach kürzen
                    planToAnalyze = executionPlan[..MaxDirectChars];
                    wasTruncated = true;
                }
            }

            var systemPrompt = language == "de"
                ? "Du bist ein erfahrener SQL Server Datenbankexperte. Analysiere den folgenden MSSQL-Ausführungsplan und gib eine detaillierte, verständliche Analyse. Erkläre: 1) Was der Plan macht, 2) Wo die teuersten Operationen sind, 3) Konkrete Optimierungsvorschläge mit Beispiel-SQL. Formatiere die Antwort mit Markdown."
                : "You are an experienced SQL Server database expert. Analyze the following MSSQL execution plan and provide a detailed, understandable analysis. Explain: 1) What the plan does, 2) Where the most expensive operations are, 3) Concrete optimization suggestions with example SQL. Format the response with Markdown.";

            if (wasTruncated)
            {
                var truncNote = language == "de"
                    ? "\n\nHINWEIS: Der ursprüngliche Plan war zu groß. Es wurden automatisch die teuersten Operationen extrahiert. Weise den Nutzer darauf hin, dass dies ein Auszug ist."
                    : "\n\nNOTE: The original plan was too large. The most expensive operations were automatically extracted. Let the user know this is an excerpt.";
                systemPrompt += truncNote;
            }

            var requestBody = new
            {
                model = _settings.Model,
                max_tokens = 4096,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = planToAnalyze }
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

            return (textContent ?? "Keine Antwort erhalten.", wasTruncated);
        }

        private string? TryExtractExpensiveOperations(string xmlPlan, string language)
        {
            try
            {
                var xmlDoc = XDocument.Parse(xmlPlan);
                XNamespace ns = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";

                // Alle RelOp-Elemente mit Kosten sammeln
                var relOps = xmlDoc.Descendants(ns + "RelOp")
                    .Select(r => new
                    {
                        Element = r,
                        Cost = double.TryParse(r.Attribute("EstimatedTotalSubtreeCost")?.Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var c) ? c : 0,
                        PhysicalOp = r.Attribute("PhysicalOp")?.Value ?? "Unknown",
                        LogicalOp = r.Attribute("LogicalOp")?.Value ?? "Unknown",
                        EstimatedRows = r.Attribute("EstimateRows")?.Value ?? "?",
                        EstimatedIo = r.Attribute("EstimatedIO")?.Value,
                        EstimatedCpu = r.Attribute("EstimateCPU")?.Value
                    })
                    .OrderByDescending(r => r.Cost)
                    .ToList();

                if (relOps.Count == 0)
                    return null;

                // SQL-Statements extrahieren
                var statements = xmlDoc.Descendants(ns + "StmtSimple")
                    .Select(s => new
                    {
                        StatementText = s.Attribute("StatementText")?.Value,
                        StatementType = s.Attribute("StatementType")?.Value,
                        TotalCost = double.TryParse(s.Attribute("StatementSubTreeCost")?.Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var c) ? c : 0
                    })
                    .Where(s => !string.IsNullOrEmpty(s.StatementText))
                    .OrderByDescending(s => s.TotalCost)
                    .Take(5)
                    .ToList();

                // Missing Indexes extrahieren
                var missingIndexes = xmlDoc.Descendants(ns + "MissingIndex")
                    .Select(mi => new
                    {
                        Table = mi.Attribute("Table")?.Value,
                        Schema = mi.Attribute("Schema")?.Value,
                        Database = mi.Attribute("Database")?.Value,
                        Columns = mi.Elements()
                            .SelectMany(g => g.Elements(ns + "ColumnGroup")?.Elements(ns + "Column") ?? g.Elements(ns + "Column"))
                            .Select(c => c.Attribute("Name")?.Value)
                            .Where(n => n != null)
                            .ToList()
                    })
                    .ToList();

                // Warnings extrahieren
                var warnings = xmlDoc.Descendants(ns + "Warnings")
                    .SelectMany(w => w.Elements())
                    .Select(w => $"{w.Name.LocalName}: {string.Join(", ", w.Attributes().Select(a => $"{a.Name}={a.Value}"))}")
                    .Distinct()
                    .ToList();

                // Top 20 teuerste Operationen
                var topOps = relOps.Take(20);

                var header = language == "de"
                    ? "=== AUTOMATISCH EXTRAHIERTE ZUSAMMENFASSUNG (Original-Plan war zu groß) ===\n\n"
                    : "=== AUTOMATICALLY EXTRACTED SUMMARY (Original plan was too large) ===\n\n";

                var sb = new StringBuilder(header);

                sb.AppendLine($"Gesamte RelOp-Knoten im Plan: {relOps.Count}");
                sb.AppendLine();

                // Statements
                if (statements.Count > 0)
                {
                    sb.AppendLine("--- SQL STATEMENTS (Top 5 by Cost) ---");
                    foreach (var stmt in statements)
                    {
                        sb.AppendLine($"  Type: {stmt.StatementType}, Cost: {stmt.TotalCost:F4}");
                        sb.AppendLine($"  SQL: {stmt.StatementText?.Trim()}");
                        sb.AppendLine();
                    }
                }

                // Top Operationen
                sb.AppendLine("--- TOP 20 EXPENSIVE OPERATIONS ---");
                foreach (var op in topOps)
                {
                    sb.AppendLine($"  PhysicalOp: {op.PhysicalOp}, LogicalOp: {op.LogicalOp}");
                    sb.AppendLine($"  EstimatedCost: {op.Cost:F4}, EstimatedRows: {op.EstimatedRows}");
                    if (op.EstimatedIo != null) sb.AppendLine($"  EstimatedIO: {op.EstimatedIo}, EstimatedCPU: {op.EstimatedCpu}");

                    // OutputList (Spalten)
                    XNamespace ns2 = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";
                    var outputCols = op.Element.Descendants(ns2 + "OutputList")
                        .FirstOrDefault()?
                        .Descendants(ns2 + "ColumnReference")
                        .Select(c => $"{c.Attribute("Table")?.Value}.{c.Attribute("Column")?.Value}")
                        .Take(10)
                        .ToList();
                    if (outputCols?.Count > 0)
                        sb.AppendLine($"  Columns: {string.Join(", ", outputCols)}");

                    // Object (Tabelle/Index)
                    var obj = op.Element.Descendants(ns2 + "Object").FirstOrDefault();
                    if (obj != null)
                    {
                        var table = obj.Attribute("Table")?.Value;
                        var index = obj.Attribute("Index")?.Value;
                        if (table != null) sb.AppendLine($"  Table: {table}, Index: {index ?? "N/A"}");
                    }

                    sb.AppendLine();
                }

                // Missing Indexes
                if (missingIndexes.Count > 0)
                {
                    sb.AppendLine("--- MISSING INDEXES (vom Query Optimizer vorgeschlagen) ---");
                    foreach (var mi in missingIndexes)
                    {
                        sb.AppendLine($"  Table: {mi.Database}.{mi.Schema}.{mi.Table}");
                        if (mi.Columns.Count > 0)
                            sb.AppendLine($"  Columns: {string.Join(", ", mi.Columns)}");
                        sb.AppendLine();
                    }
                }

                // Warnings
                if (warnings.Count > 0)
                {
                    sb.AppendLine("--- WARNINGS ---");
                    foreach (var w in warnings)
                        sb.AppendLine($"  {w}");
                    sb.AppendLine();
                }

                var result = sb.ToString();
                _logger.LogInformation("Plan extrahiert: {OriginalLength} -> {ExtractedLength} Zeichen, {OpCount} Operationen",
                    xmlPlan.Length, result.Length, relOps.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "XML-Parsing des Ausführungsplans fehlgeschlagen, falle auf Textkürzung zurück");
                return null;
            }
        }
    }
}
