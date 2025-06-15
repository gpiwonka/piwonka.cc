// Pages/Admin/Seiten/Edit.cshtml.cs
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

namespace Piwonka.CC.Pages.Admin.Seiten
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class EditModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMenuService _menuService;

        public EditModel(IDbContextFactory<ApplicationDbContext> contextFactory, IMenuService menuService)
        {
            _contextFactory = contextFactory;
            _menuService = menuService;
        }

        [BindProperty]
        public SeiteEditViewModel SeiteViewModel { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var seite = await context.Seiten.FindAsync(id);

            if (seite == null)
            {
                return NotFound();
            }

            // Verfügbare Parent-Seiten laden (außer der aktuellen Seite)
            var verfuegbareParents = await _menuService.GetSeitenHierarchyAsync(id);

            // Entity zu ViewModel konvertieren
            SeiteViewModel = new SeiteEditViewModel
            {
                Id = seite.Id,
                Titel = seite.Titel,
                Slug = seite.Slug,
                Inhalt = seite.Inhalt,
                MetaDescription = seite.MetaDescription,
                MetaKeywords = seite.MetaKeywords,
                IstVeroeffentlicht = seite.IstVeroeffentlicht,
                ImMenuAnzeigen = seite.ImMenuAnzeigen,
                Reihenfolge = seite.Reihenfolge,
                Template = seite.Template,
                ErstelltAm = seite.ErstelltAm,
                ParentId = seite.ParentId,
                MenuGruppe = seite.MenuGruppe,
                MenuTitel = seite.MenuTitel,
                IstMenuGruppe = seite.IstMenuGruppe,
                VerfuegbareParents = verfuegbareParents
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verfügbare Parents für den Fall eines Validation-Fehlers neu laden
            SeiteViewModel.VerfuegbareParents = await _menuService.GetSeitenHierarchyAsync(SeiteViewModel.Id);

            // Explizit Inhalt aus Form holen (für TinyMCE)
            var formInhalt = Request.Form["SeiteViewModel.Inhalt"].ToString();
            if (string.IsNullOrEmpty(SeiteViewModel.Inhalt) && !string.IsNullOrEmpty(formInhalt))
            {
                SeiteViewModel.Inhalt = formInhalt;
            }

            using var context = _contextFactory.CreateDbContext();

            // Prüfen, ob Slug bereits existiert (außer bei der aktuellen Seite)
            var existingSlug = await context.Seiten
                .AnyAsync(s => s.Slug == SeiteViewModel.Slug && s.Id != SeiteViewModel.Id);
            if (existingSlug)
            {
                ModelState.AddModelError("SeiteViewModel.Slug", "Dieser Slug ist bereits vergeben.");
            }

            // Prüfen auf zirkuläre Referenzen bei Parent-Child-Beziehungen
            if (SeiteViewModel.ParentId.HasValue)
            {
                var wouldCreateCircle = await WouldCreateCircularReference(context, SeiteViewModel.Id, SeiteViewModel.ParentId.Value);
                if (wouldCreateCircle)
                {
                    ModelState.AddModelError("SeiteViewModel.ParentId", "Diese Auswahl würde eine zirkuläre Referenz erstellen.");
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Existierende Entity aus der Datenbank laden
            var existingSeite = await context.Seiten.FindAsync(SeiteViewModel.Id);
            if (existingSeite == null)
            {
                return NotFound();
            }

            // ViewModel-Daten auf Entity übertragen
            existingSeite.Titel = SeiteViewModel.Titel;
            existingSeite.Slug = SeiteViewModel.Slug;
            existingSeite.Inhalt = SeiteViewModel.Inhalt;
            existingSeite.MetaDescription = SeiteViewModel.MetaDescription;
            existingSeite.MetaKeywords = SeiteViewModel.MetaKeywords;
            existingSeite.IstVeroeffentlicht = SeiteViewModel.IstVeroeffentlicht;
            existingSeite.ImMenuAnzeigen = SeiteViewModel.ImMenuAnzeigen;
            existingSeite.Reihenfolge = SeiteViewModel.Reihenfolge;
            existingSeite.Template = SeiteViewModel.Template;
            existingSeite.BearbeitetAm = DateTime.Now;
            existingSeite.ParentId = SeiteViewModel.ParentId;
            existingSeite.MenuGruppe = SeiteViewModel.MenuGruppe;
            existingSeite.MenuTitel = SeiteViewModel.MenuTitel;
            existingSeite.IstMenuGruppe = SeiteViewModel.IstMenuGruppe;

            context.Entry(existingSeite).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Die Seite wurde erfolgreich aktualisiert.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SeiteExists(context, SeiteViewModel.Id))
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

        private async Task<bool> SeiteExists(ApplicationDbContext context, int id)
        {
            return await context.Seiten.AnyAsync(e => e.Id == id);
        }

        private async Task<bool> WouldCreateCircularReference(ApplicationDbContext context, int seiteId, int parentId)
        {
            var currentParentId = parentId;

            while (currentParentId != 0)
            {
                if (currentParentId == seiteId)
                {
                    return true; // Zirkuläre Referenz gefunden
                }

                var parent = await context.Seiten.FindAsync(currentParentId);
                if (parent?.ParentId == null)
                {
                    break;
                }

                currentParentId = parent.ParentId.Value;
            }

            return false;
        }
    }
}