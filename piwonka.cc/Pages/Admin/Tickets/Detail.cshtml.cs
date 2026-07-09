using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.Tickets
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class DetailModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<DetailModel> _logger;

        public DetailModel(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<DetailModel> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        [BindProperty]
        public Ticket Ticket { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null) return NotFound();
            Ticket = entity;
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null) return NotFound();

            entity.Status = Ticket.Status;
            entity.AdminNotiz = Ticket.AdminNotiz;
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ticket aktualisiert.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null) return NotFound();

            context.Tickets.Remove(entity);
            await context.SaveChangesAsync();

            _logger.LogInformation("Ticket #{Id} gelöscht", id);
            TempData["SuccessMessage"] = "Ticket gelöscht.";
            return RedirectToPage("./Index");
        }
    }
}
