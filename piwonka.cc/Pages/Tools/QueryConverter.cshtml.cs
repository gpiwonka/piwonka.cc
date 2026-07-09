using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Services;
using System.Security.Cryptography;

namespace Piwonka.CC.Pages.Tools
{
    public class QueryConverterModel : PageModel
    {
        private readonly DbaToolService _dbaToolService;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<QueryConverterModel> _logger;

        public QueryConverterModel(
            DbaToolService dbaToolService,
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<QueryConverterModel> logger)
        {
            _dbaToolService = dbaToolService;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        [BindProperty]
        public string SqlInput { get; set; } = string.Empty;

        [BindProperty]
        public string SourceDialect { get; set; } = "T-SQL (SQL Server)";

        [BindProperty]
        public string TargetDialect { get; set; } = "PostgreSQL";

        [BindProperty]
        public string Language { get; set; } = "de";

        [BindProperty]
        public string? AnalysisResult { get; set; }

        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
        public bool AnalysisComplete { get; set; }

        public static readonly string[] Dialects = new[]
        {
            "T-SQL (SQL Server)",
            "PostgreSQL",
            "MySQL",
            "Oracle PL/SQL",
            "SQLite",
            "MariaDB"
        };

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(SqlInput))
            {
                HasError = true;
                ErrorMessage = Language == "de"
                    ? "Bitte eine SQL-Abfrage einfügen."
                    : "Please paste a SQL query.";
                return Page();
            }

            if (SourceDialect == TargetDialect)
            {
                HasError = true;
                ErrorMessage = Language == "de"
                    ? "Quell- und Zieldialekt dürfen nicht gleich sein."
                    : "Source and target dialect must be different.";
                return Page();
            }

            try
            {
                AnalysisResult = await _dbaToolService.ConvertQueryAsync(SqlInput, SourceDialect, TargetDialect, Language);
                AnalysisComplete = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei der Query-Konvertierung");
                HasError = true;
                ErrorMessage = Language == "de"
                    ? "Fehler bei der Konvertierung. Bitte versuchen Sie es erneut."
                    : "Error during conversion. Please try again.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostShareAsync()
        {
            if (string.IsNullOrWhiteSpace(SqlInput) || string.IsNullOrWhiteSpace(AnalysisResult))
            {
                HasError = true;
                ErrorMessage = "Kein Ergebnis zum Teilen vorhanden.";
                return Page();
            }

            try
            {
                var hash = Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant();
                using var context = await _contextFactory.CreateDbContextAsync();
                context.PlanAdviceResults.Add(new PlanAdviceResult
                {
                    Hash = hash,
                    ExecutionPlan = $"-- {SourceDialect} -> {TargetDialect}\n{SqlInput}",
                    AnalysisResult = AnalysisResult,
                    Language = Language,
                    ToolType = "QueryConverter"
                });
                await context.SaveChangesAsync();
                return RedirectToPage("/PlanResult", new { hash });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Speichern");
                HasError = true;
                AnalysisComplete = true;
                ErrorMessage = "Fehler beim Speichern. Bitte erneut versuchen.";
                return Page();
            }
        }
    }
}
