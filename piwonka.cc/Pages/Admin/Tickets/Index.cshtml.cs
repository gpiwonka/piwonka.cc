using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.Tickets
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<Ticket> Tickets { get; set; } = new List<Ticket>();

        [BindProperty(SupportsGet = true)]
        public TicketStatus? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public TicketTyp? TypFilter { get; set; }

        public int OffenCount { get; set; }
        public int InBearbeitungCount { get; set; }
        public int GeschlossenCount { get; set; }

        public async Task OnGetAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            OffenCount = await context.Tickets.CountAsync(t => t.Status == TicketStatus.Offen);
            InBearbeitungCount = await context.Tickets.CountAsync(t => t.Status == TicketStatus.InBearbeitung);
            GeschlossenCount = await context.Tickets.CountAsync(t => t.Status == TicketStatus.Geschlossen);

            var query = context.Tickets.AsQueryable();
            if (StatusFilter.HasValue) query = query.Where(t => t.Status == StatusFilter.Value);
            if (TypFilter.HasValue) query = query.Where(t => t.Typ == TypFilter.Value);

            Tickets = await query
                .OrderByDescending(t => t.ErstelltAm)
                .ToListAsync();
        }
    }
}
