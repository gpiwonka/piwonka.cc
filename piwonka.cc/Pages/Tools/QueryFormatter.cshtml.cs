using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Services;
using System.Security.Cryptography;

namespace Piwonka.CC.Pages.Tools
{
    public class QueryFormatterModel : PageModel
    {
        private readonly DbaToolService _dbaToolService;
        private readonly SqlFormatterService _sqlFormatterService;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<QueryFormatterModel> _logger;

        public QueryFormatterModel(
            DbaToolService dbaToolService,
            SqlFormatterService sqlFormatterService,
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<QueryFormatterModel> logger)
        {
            _dbaToolService = dbaToolService;
            _sqlFormatterService = sqlFormatterService;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        [BindProperty]
        public string SqlInput { get; set; } = string.Empty;

        [BindProperty]
        public string Language { get; set; } = "de";

        [BindProperty]
        public bool UseAi { get; set; }

        [BindProperty]
        public string? AnalysisResult { get; set; }

        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
        public bool AnalysisComplete { get; set; }
        public bool UsedAi { get; set; }

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

            try
            {
                if (UseAi)
                {
                    AnalysisResult = await _dbaToolService.FormatQueryAsync(SqlInput, Language);
                    UsedAi = true;
                }
                else
                {
                    var formatted = _sqlFormatterService.Format(SqlInput);
                    AnalysisResult = $"```sql\n{formatted}\n```";
                }
                AnalysisComplete = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Formatieren der Query");
                HasError = true;
                ErrorMessage = Language == "de"
                    ? "Fehler bei der Analyse. Bitte versuchen Sie es erneut."
                    : "Error during analysis. Please try again.";
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
                    ExecutionPlan = SqlInput,
                    AnalysisResult = AnalysisResult,
                    Language = Language,
                    ToolType = "QueryFormatter"
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
