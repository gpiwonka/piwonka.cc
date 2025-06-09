using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Piwonka.CC.Pages.admin.Posts
{

    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Post Post { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadKategorien();

                return Page();
            }

            _context.Posts.Add(Post);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public SelectList KategorieOptions { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadKategorien();
            return Page();
        }


        private async Task LoadKategorien()
        {
            var kategorien = await _context.Kategorien.ToListAsync();
            KategorieOptions = new SelectList(kategorien, "Id", "Name");
        }
    }
}

