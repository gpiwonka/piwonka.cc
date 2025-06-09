
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;
using Piwonka.CC.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages.Admin.Posts
{
    [TypeFilter(typeof(AdminAuthFilter))]
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
            // Debug: Schauen wir, was im Post.Inhalt steht
            System.Diagnostics.Debug.WriteLine($"Post.Inhalt beim Submit: {Post.Inhalt?.Substring(0, Math.Min(100, Post.Inhalt?.Length ?? 0))}...");

            // Explizit den Inhalt aus dem Request lesen, falls der Model Binder versagt
            if (string.IsNullOrEmpty(Post.Inhalt))
            {
                var formInhalt = Request.Form["Post.Inhalt"].ToString();
                if (!string.IsNullOrEmpty(formInhalt))
                {
                    Post.Inhalt = formInhalt;
                    System.Diagnostics.Debug.WriteLine($"Inhalt aus Form geholt: {Post.Inhalt.Substring(0, Math.Min(100, Post.Inhalt.Length))}...");
                }
            }

            if (!ModelState.IsValid)
            {
                // Debug: ModelState-Fehler ausgeben
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }

                await LoadKategorien();
                return Page();
            }

            // Den existierenden Post aus der Datenbank laden
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
            existingPost.Inhalt = Post.Inhalt; // Das sollte jetzt den Editor-Inhalt haben
            existingPost.KategorieId = Post.KategorieId;
            existingPost.IstVeroeffentlicht = Post.IstVeroeffentlicht;
            existingPost.BildUrl = Post.BildUrl;

            // Slug aktualisieren, falls der Titel geändert wurde
            if (string.IsNullOrEmpty(existingPost.Slug) ||
                !string.Equals(existingPost.Titel, Post.Titel))
            {
                existingPost.Slug = SlugGenerator.GenerateSlug(Post.Titel);
            }

            // Debug: Finaler Inhalt vor dem Speichern
            System.Diagnostics.Debug.WriteLine($"Finaler Inhalt vor dem Speichern: {existingPost.Inhalt?.Substring(0, Math.Min(100, existingPost.Inhalt?.Length ?? 0))}...");

            _context.Entry(existingPost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Post erfolgreich gespeichert!");
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