
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages.Admin.IndexNow
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IIndexNowService _indexNowService;
        private readonly IConfiguration _configuration;

        public IndexModel(
            ApplicationDbContext context,
            IIndexNowService indexNowService,
            IConfiguration configuration)
        {
            _context = context;
            _indexNowService = indexNowService;
            _configuration = configuration;
        }

        // Properties für die View
        public bool IsEnabled { get; set; }
        public string Host { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string KeyFileUrl { get; set; } = string.Empty;
        public int PublishedPagesCount { get; set; }
        public int PublishedPostsCount { get; set; }
        public int TotalUrls { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostNotifyUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                TempData["ErrorMessage"] = "Bitte geben Sie eine gültige URL ein.";
                await LoadDataAsync();
                return Page();
            }

            try
            {
                await _indexNowService.NotifyUrlAsync(url);
                TempData["SuccessMessage"] = $"URL wurde erfolgreich an IndexNow gesendet: {url}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Fehler beim Senden der URL: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostNotifyAllPagesAsync()
        {
            try
            {
                var publishedPages = await _context.Seiten
                    .Where(s => s.IstVeroeffentlicht)
                    .Select(s => s.Slug)
                    .ToListAsync();

                var urls = publishedPages.Select(slug => $"https://{Host}/seite/{slug}").ToList();

                if (urls.Any())
                {
                    await _indexNowService.NotifyUrlsAsync(urls);
                    TempData["SuccessMessage"] = $"{urls.Count} Seiten wurden erfolgreich an IndexNow gesendet.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Keine veröffentlichten Seiten gefunden.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Fehler beim Senden der Seiten: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostNotifyAllPostsAsync()
        {
            try
            {
                var publishedPosts = await _context.Posts
                    .Where(p => p.IstVeroeffentlicht)
                    .Select(p => p.Slug)
                    .ToListAsync();

                var urls = publishedPosts.Select(slug => $"https://{Host}/blog/{slug}").ToList();

                if (urls.Any())
                {
                    await _indexNowService.NotifyUrlsAsync(urls);
                    TempData["SuccessMessage"] = $"{urls.Count} Blog-Posts wurden erfolgreich an IndexNow gesendet.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Keine veröffentlichten Blog-Posts gefunden.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Fehler beim Senden der Blog-Posts: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostNotifySitemapAsync()
        {
            try
            {
                await _indexNowService.NotifySitemapUpdatedAsync();
                TempData["SuccessMessage"] = "Sitemap wurde erfolgreich an IndexNow gesendet.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Fehler beim Senden der Sitemap: {ex.Message}";
            }

            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            // Konfiguration laden
            IsEnabled = _configuration.GetValue<bool>("IndexNow:Enabled", true);
            Host = _configuration["IndexNow:Host"] ?? "piwonka.cc";

            // API Key von Service holen
            if (_indexNowService is IndexNowService indexNowService)
            {
                ApiKey = indexNowService.GetApiKey();
            }

            KeyFileUrl = $"https://{Host}/{ApiKey}.txt";

            // Statistiken laden
            PublishedPagesCount = await _context.Seiten.CountAsync(s => s.IstVeroeffentlicht);
            PublishedPostsCount = await _context.Posts.CountAsync(p => p.IstVeroeffentlicht);
            TotalUrls = PublishedPagesCount + PublishedPostsCount + 1; // +1 für Sitemap
        }
    }
}

