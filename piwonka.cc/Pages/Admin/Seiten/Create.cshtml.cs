// Pages/Admin/Seiten/Create.cshtml.cs (Update mit Language)
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.ViewModels;
using Piwonka.CC.Services;
using Piwonka.CC.Filters;
using System.Text.RegularExpressions;

namespace Piwonka.CC.Pages.Admin.Seiten
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class CreateModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMenuService _menuService;

        public CreateModel(IDbContextFactory<ApplicationDbContext> contextFactory, IMenuService menuService)
        {
            _contextFactory = contextFactory;
            _menuService = menuService;
        }

        [BindProperty]
        public SeiteCreateViewModel SeiteViewModel { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verfügbare Parent-Seiten laden
            var verfuegbareParents = await _menuService.GetSeitenHierarchyAsync();

            // ViewModel mit Standardwerten initialisieren
            SeiteViewModel = new SeiteCreateViewModel
            {
                Template = "Standard",
                IstVeroeffentlicht = true,
                ImMenuAnzeigen = false,
                Reihenfolge = 0,
                Language = Language.DE, // Standardsprache
                VerfuegbareParents = verfuegbareParents
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verfügbare Parents für den Fall eines Validation-Fehlers neu laden
            SeiteViewModel.VerfuegbareParents = await _menuService.GetSeitenHierarchyAsync();

            // Explizit Inhalt aus Form holen (für TinyMCE)
            var formInhalt = Request.Form["SeiteViewModel.Inhalt"].ToString();
            if (string.IsNullOrEmpty(SeiteViewModel.Inhalt) && !string.IsNullOrEmpty(formInhalt))
            {
                SeiteViewModel.Inhalt = formInhalt;
            }

            // Slug automatisch generieren, wenn leer
            if (string.IsNullOrWhiteSpace(SeiteViewModel.Slug))
            {
                SeiteViewModel.Slug = GenerateSlug(SeiteViewModel.Titel);
            }
            else
            {
                SeiteViewModel.Slug = GenerateSlug(SeiteViewModel.Slug);
            }

            // Prüfen, ob Slug bereits existiert (pro Sprache)
            if (!string.IsNullOrWhiteSpace(SeiteViewModel.Slug))
            {
                using var _context = await _contextFactory.CreateDbContextAsync();
                var existingSlug = await _context.Seiten
                    .AnyAsync(s => s.Slug == SeiteViewModel.Slug && s.Language == SeiteViewModel.Language);
                if (existingSlug)
                {
                    ModelState.AddModelError("SeiteViewModel.Slug", "Dieser Slug ist bereits in dieser Sprache vergeben.");
                }
            }

            if (string.IsNullOrWhiteSpace(SeiteViewModel.Titel))
            {
                ModelState.AddModelError("SeiteViewModel.Titel", "Der Titel ist erforderlich.");
            }

            if (string.IsNullOrWhiteSpace(SeiteViewModel.Slug))
            {
                ModelState.AddModelError("SeiteViewModel.Slug", "Der Slug konnte nicht generiert werden.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ViewModel zu Entity konvertieren
            var seite = new Seite
            {
                Titel = SeiteViewModel.Titel.Trim(),
                Slug = SeiteViewModel.Slug.Trim(),
                Inhalt = SeiteViewModel.Inhalt ?? string.Empty,
                MetaDescription = string.IsNullOrWhiteSpace(SeiteViewModel.MetaDescription) ? null : SeiteViewModel.MetaDescription.Trim(),
                MetaKeywords = string.IsNullOrWhiteSpace(SeiteViewModel.MetaKeywords) ? null : SeiteViewModel.MetaKeywords.Trim(),
                IstVeroeffentlicht = SeiteViewModel.IstVeroeffentlicht,
                ImMenuAnzeigen = SeiteViewModel.ImMenuAnzeigen,
                Reihenfolge = SeiteViewModel.Reihenfolge,
                Template = SeiteViewModel.Template ?? "Standard",
                Language = SeiteViewModel.Language, // Neue Language Property
                ErstelltAm = DateTime.Now,
                BearbeitetAm = null,
                ParentId = SeiteViewModel.ParentId,
                MenuGruppe = string.IsNullOrWhiteSpace(SeiteViewModel.MenuGruppe) ? null : SeiteViewModel.MenuGruppe.Trim(),
                MenuTitel = string.IsNullOrWhiteSpace(SeiteViewModel.MenuTitel) ? null : SeiteViewModel.MenuTitel.Trim(),
                IstMenuGruppe = SeiteViewModel.IstMenuGruppe
            };

            try
            {
                using var _context = await _contextFactory.CreateDbContextAsync();      
                _context.Seiten.Add(seite);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Die Seite '{seite.Titel}' wurde erfolgreich erstellt.";
                return RedirectToPage("./Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Fehler beim Speichern der Seite.");
                return Page();
            }
        }

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.Replace("ä", "ae")
                      .Replace("ö", "oe")
                      .Replace("ü", "ue")
                      .Replace("Ä", "ae")
                      .Replace("Ö", "oe")
                      .Replace("Ü", "ue")
                      .Replace("ß", "ss")
                      .Replace("&", "und");

            text = text.ToLowerInvariant();
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = Regex.Replace(text, @"[\s-]+", "-");
            text = text.Trim('-');

            return text;
        }
    }
}