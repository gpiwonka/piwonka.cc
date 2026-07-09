using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages
{
    public class PlanResultModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public PlanResultModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public PlanAdviceResult? Result { get; set; }
        public bool IsExpired { get; set; }

        public async Task<IActionResult> OnGetAsync(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return NotFound();

            using var context = await _contextFactory.CreateDbContextAsync();
            Result = await context.PlanAdviceResults
                .FirstOrDefaultAsync(p => p.Hash == hash);

            if (Result == null)
                return NotFound();

            if (DateTime.Now > Result.AblaufAm)
            {
                IsExpired = true;
                return Page();
            }

            return Page();
        }
    }
}
