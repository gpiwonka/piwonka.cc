using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages.Blog
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Post Post { get; set; }
        public Post PreviousPost { get; set; }
        public Post NextPost { get; set; }
        public IList<Post> RecentPosts { get; set; }
        public IList<Post> RelatedPosts { get; set; }
        public IList<Kategorie> Kategorien { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, string slug = null)
        {
            // Den angeforderten Post laden
            Post = await _context.Posts
                .Include(p => p.Kategorie)
                .FirstOrDefaultAsync(m => m.Id == id && m.IstVeroeffentlicht);

            if (Post == null)
            {
                return NotFound();
            }

            // Redirect zur korrekten Slug-URL, falls ein falscher Slug angegeben wurde
            if (!string.IsNullOrEmpty(Post.Slug) && !string.IsNullOrEmpty(slug) && slug != Post.Slug)
            {
                return RedirectToPage("./Detail", new { id = Post.Id, slug = Post.Slug });
            }

            // Vorherigen und nächsten Post laden
            PreviousPost = await _context.Posts
                .Where(p => p.IstVeroeffentlicht && p.ErstelltAm < Post.ErstelltAm)
                .OrderByDescending(p => p.ErstelltAm)
                .FirstOrDefaultAsync();

            NextPost = await _context.Posts
                .Where(p => p.IstVeroeffentlicht && p.ErstelltAm > Post.ErstelltAm)
                .OrderBy(p => p.ErstelltAm)
                .FirstOrDefaultAsync();

            // Neueste Posts laden (außer dem aktuellen)
            RecentPosts = await _context.Posts
                .Where(p => p.IstVeroeffentlicht && p.Id != Post.Id)
                .OrderByDescending(p => p.ErstelltAm)
                .Take(5)
                .ToListAsync();

            // Verwandte Posts laden (aus derselben Kategorie, außer dem aktuellen)
            if (Post.KategorieId.HasValue)
            {
                RelatedPosts = await _context.Posts
                    .Where(p => p.IstVeroeffentlicht && p.Id != Post.Id && p.KategorieId == Post.KategorieId)
                    .OrderByDescending(p => p.ErstelltAm)
                    .Take(4)
                    .ToListAsync();
            }
            else
            {
                RelatedPosts = new List<Post>();
            }

            // Kategorien laden
            Kategorien = await _context.Kategorien
                .OrderBy(k => k.Name)
                .ToListAsync();

            return Page();
        }
    }
}