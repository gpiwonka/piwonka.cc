using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.Services
{
    public interface IMenuService
    {
        Task<MenuViewModel> GetMenuAsync();
        Task<List<SeiteOption>> GetSeitenHierarchyAsync(int? excludeId = null);
    }

    public class MenuService : IMenuService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public MenuService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<MenuViewModel> GetMenuAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            var seiten = await context.Seiten
                .Where(s => s.IstVeroeffentlicht && s.ImMenuAnzeigen)
                .OrderBy(s => s.Reihenfolge)
                .ThenBy(s => s.Titel)
                .ToListAsync();

            var menuViewModel = new MenuViewModel();

            // Gruppierte Seiten verarbeiten
            var gruppierteSeiten = seiten
                .Where(s => !string.IsNullOrEmpty(s.MenuGruppe))
                .GroupBy(s => s.MenuGruppe);

            foreach (var gruppe in gruppierteSeiten)
            {
                var menuGroup = new MenuGroup
                {
                    Name = gruppe.Key!
                };

                // Prüfen ob es eine Hauptseite für diese Gruppe gibt
                var hauptSeite = gruppe.FirstOrDefault(s => s.IstMenuGruppe);
                if (hauptSeite != null && !hauptSeite.IstMenuGruppe)
                {
                    menuGroup.Url = $"/seite/{hauptSeite.Slug}";
                }

                // Unterseiten hinzufügen
                menuGroup.Items = gruppe
                    .Where(s => !s.IstMenuGruppe)
                    .Select(s => new MenuItemViewModel
                    {
                        Title = s.MenuTitel ?? s.Titel,
                        Url = $"/seite/{s.Slug}",
                        Reihenfolge = s.Reihenfolge
                    })
                    .OrderBy(m => m.Reihenfolge)
                    .ThenBy(m => m.Title)
                    .ToList();

                menuViewModel.MenuGroups.Add(menuGroup);
            }

            // Top-Level Seiten (ohne Gruppe)
            menuViewModel.TopLevelItems = seiten
                .Where(s => string.IsNullOrEmpty(s.MenuGruppe) && !s.ParentId.HasValue)
                .Select(s => new MenuItemViewModel
                {
                    Title = s.MenuTitel ?? s.Titel,
                    Url = $"/seite/{s.Slug}",
                    Reihenfolge = s.Reihenfolge,
                    Children = GetChildren(seiten, s.Id)
                })
                .OrderBy(m => m.Reihenfolge)
                .ThenBy(m => m.Title)
                .ToList();

            return menuViewModel;
        }

        private List<MenuItemViewModel> GetChildren(List<Seite> allSeiten, int parentId)
        {
            return allSeiten
                .Where(s => s.ParentId == parentId)
                .Select(s => new MenuItemViewModel
                {
                    Title = s.MenuTitel ?? s.Titel,
                    Url = $"/seite/{s.Slug}",
                    Reihenfolge = s.Reihenfolge,
                    Children = GetChildren(allSeiten, s.Id)
                })
                .OrderBy(m => m.Reihenfolge)
                .ThenBy(m => m.Title)
                .ToList();
        }

        public async Task<List<SeiteOption>> GetSeitenHierarchyAsync(int? excludeId = null)
        {
            using var context = _contextFactory.CreateDbContext();

            var seiten = await context.Seiten
                .Where(s => s.Id != excludeId)
                .OrderBy(s => s.Reihenfolge)
                .ThenBy(s => s.Titel)
                .ToListAsync();

            var result = new List<SeiteOption>();
            BuildHierarchy(seiten, result, null, 0);
            return result;
        }

        private void BuildHierarchy(List<Seite> allSeiten, List<SeiteOption> result, int? parentId, int level)
        {
            var children = allSeiten.Where(s => s.ParentId == parentId).ToList();

            foreach (var child in children)
            {
                result.Add(new SeiteOption
                {
                    Id = child.Id,
                    Titel = child.Titel,
                    Level = level
                });

                BuildHierarchy(allSeiten, result, child.Id, level + 1);
            }
        }
    }
}