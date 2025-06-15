using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Data;

namespace Piwonka.CC.Pages.admin.Posts
{
    public class DeleteModel : PageModel
    {
        IDbContextFactory<ApplicationDbContext> _contextFactory;

        public DeleteModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory    ;
        }

        [BindProperty]
        public Post Post { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var _context = _contextFactory.CreateDbContext();     
            Post = await _context.Posts.FindAsync(id);

            if (Post == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var _context = _contextFactory.CreateDbContext(); 
            Post = await _context.Posts.FindAsync(Post.Id);

            if (Post != null)
            {
                _context.Posts.Remove(Post);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}