using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Services;
using System.Security.Cryptography;

namespace Piwonka.CC.Pages.Tools
{
    public class IndexAdvisorModel : PageModel
    {
        private readonly DbaToolService _dbaToolService;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<IndexAdvisorModel> _logger;

        public IndexAdvisorModel(
            DbaToolService dbaToolService,
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<IndexAdvisorModel> logger)
        {
            _dbaToolService = dbaToolService;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        [BindProperty]
        public string DdlAndQueries { get; set; } = string.Empty;

        [BindProperty]
        public string Language { get; set; } = "de";

        [BindProperty]
        public string? AnalysisResult { get; set; }

        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
        public bool AnalysisComplete { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(DdlAndQueries))
            {
                HasError = true;
                ErrorMessage = Language == "de"
                    ? "Bitte Tabellendefinitionen und/oder Queries einfügen."
                    : "Please paste table definitions and/or queries.";
                return Page();
            }

            try
            {
                AnalysisResult = await _dbaToolService.AnalyzeIndexesAsync(DdlAndQueries, Language);
                AnalysisComplete = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei der Index-Analyse");
                HasError = true;
                ErrorMessage = Language == "de"
                    ? "Fehler bei der Analyse. Bitte versuchen Sie es erneut."
                    : "Error during analysis. Please try again.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostShareAsync()
        {
            if (string.IsNullOrWhiteSpace(DdlAndQueries) || string.IsNullOrWhiteSpace(AnalysisResult))
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
                    ExecutionPlan = DdlAndQueries,
                    AnalysisResult = AnalysisResult,
                    Language = Language,
                    ToolType = "IndexAdvisor"
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
