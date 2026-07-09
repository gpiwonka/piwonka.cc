using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Services;
using System.Security.Cryptography;

namespace Piwonka.CC.Pages
{
    public class PlanAdviceModel : PageModel
    {
        private readonly PlanAdviceService _planAdviceService;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<PlanAdviceModel> _logger;

        public PlanAdviceModel(
            PlanAdviceService planAdviceService,
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<PlanAdviceModel> logger)
        {
            _planAdviceService = planAdviceService;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        [BindProperty]
        public string ExecutionPlan { get; set; } = string.Empty;

        [BindProperty]
        public string Language { get; set; } = "de";

        [BindProperty]
        public string? AnalysisResult { get; set; }

        [BindProperty]
        public bool WasTruncated { get; set; }

        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
        public bool AnalysisComplete { get; set; }
        public int CharCount { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(ExecutionPlan))
            {
                HasError = true;
                ErrorMessage = Language == "de"
                    ? "Bitte einen Ausführungsplan einfügen."
                    : "Please paste an execution plan.";
                return Page();
            }

            CharCount = ExecutionPlan.Length;

            try
            {
                var (analysis, wasTruncated) = await _planAdviceService.AnalyzeExecutionPlanAsync(ExecutionPlan, Language);
                AnalysisResult = analysis;
                WasTruncated = wasTruncated;
                AnalysisComplete = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei der Analyse des Ausführungsplans");
                HasError = true;
                ErrorMessage = Language == "de"
                    ? "Fehler bei der Analyse. Bitte versuchen Sie es erneut."
                    : "Error during analysis. Please try again.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostShareAsync()
        {
            if (string.IsNullOrWhiteSpace(ExecutionPlan) || string.IsNullOrWhiteSpace(AnalysisResult))
            {
                // Felder kommen nicht mehr mit — aus TempData holen ist nicht möglich,
                // also per Hidden Fields mitschicken
                HasError = true;
                ErrorMessage = Language == "de"
                    ? "Kein Analyse-Ergebnis zum Teilen vorhanden."
                    : "No analysis result to share.";
                return Page();
            }

            try
            {
                var hash = GenerateHash();

                using var context = await _contextFactory.CreateDbContextAsync();
                var result = new PlanAdviceResult
                {
                    Hash = hash,
                    ExecutionPlan = ExecutionPlan,
                    AnalysisResult = AnalysisResult,
                    Language = Language,
                    WasTruncated = WasTruncated
                };

                context.PlanAdviceResults.Add(result);
                await context.SaveChangesAsync();

                return RedirectToPage("/PlanResult", new { hash });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Speichern des Analyse-Ergebnisses");
                HasError = true;
                AnalysisComplete = true;
                ErrorMessage = Language == "de"
                    ? "Fehler beim Speichern. Bitte versuchen Sie es erneut."
                    : "Error saving result. Please try again.";
                return Page();
            }
        }

        private static string GenerateHash()
        {
            var bytes = RandomNumberGenerator.GetBytes(8);
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
