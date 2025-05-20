// Pages/Admin/Posts/Edit.cshtml.cs (korrigiert)
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data.Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Pages.Admin.Posts
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly FileUploadService _fileUploadService;

        public EditModel(ApplicationDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        [BindProperty]
        public Post Post { get; set; }

        public Microsoft.AspNetCore.Mvc.Rendering.SelectList KategorieOptions { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Post vollständig laden
            Post = await _context.Posts
                .Include(p => p.Kategorie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Post == null)
            {
                return NotFound();
            }

            await LoadKategorien();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadKategorien();
                return Page();
            }

            // WICHTIG: Den ursprünglichen Post aus der Datenbank laden
            var existingPost = await _context.Posts.FindAsync(Post.Id);
            if (existingPost == null)
            {
                return NotFound();
            }

            // Bild-Upload verarbeiten
            if (Post.UploadedImage != null)
            {
                Post.BildUrl = await _fileUploadService.UploadImageAsync(Post.UploadedImage);
            }
            else
            {
                // Wenn kein neues Bild hochgeladen wurde, behalten wir das alte bei
                Post.BildUrl = existingPost.BildUrl;
            }

            // Aktualisieren Sie nur die Eigenschaften, die geändert werden sollen
            existingPost.Titel = Post.Titel;
            existingPost.Inhalt = Post.Inhalt;
            existingPost.KategorieId = Post.KategorieId;
            existingPost.IstVeröffentlicht = Post.IstVeröffentlicht;
            existingPost.BildUrl = Post.BildUrl;

            // Wenn kein Slug vorhanden ist oder der Titel geändert wurde, generieren wir einen neuen
            if (string.IsNullOrEmpty(existingPost.Slug) ||
                !string.Equals(existingPost.Titel, Post.Titel))
            {
                existingPost.Slug = SlugGenerator.GenerateSlug(Post.Titel);
            }

            // Den existierenden Post markieren als geändert
            _context.Entry(existingPost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(Post.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }

        private async Task LoadKategorien()
        {
            var kategorien = await _context.Kategorien.ToListAsync();
            KategorieOptions = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                kategorien, "Id", "Name", Post?.KategorieId);
        }
    }
}