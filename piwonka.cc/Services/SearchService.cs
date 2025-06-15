using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Piwonka.CC.Services
{
    public class SearchService : ISearchService
    {
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		private readonly ILanguageService _languageService;

        public SearchService(IDbContextFactory<ApplicationDbContext> contextFactory, ILanguageService languageService)
        {
            _contextFactory = contextFactory;
            _languageService = languageService;
        }

        public async Task<SearchResultViewModel> SearchAsync(string query, Language? languageCode = null, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new SearchResultViewModel();
            }

            // Aktuelle Sprache ermitteln
            if (languageCode == null)
            {
                languageCode = await _languageService.GetCurrentLanguageAsync();
            }

            
            var searchTerms = PrepareSearchTerms(query);
            var results = new List<SearchResultItemViewModel>();

            // Seiten durchsuchen
            var seitenResults = await SearchSeitenAsync(searchTerms, languageCode.Value);
            results.AddRange(seitenResults);

            // Blog-Posts durchsuchen
            var blogResults = await SearchPostsAsync(searchTerms, languageCode.Value);
            results.AddRange(blogResults);

            // Kategorien durchsuchen
            var kategorienResults = await SearchKategorienAsync(searchTerms, languageCode.Value);
            results.AddRange(kategorienResults);

            // Relevanz sortieren und paginieren
            var sortedResults = results
                .OrderByDescending(r => r.Relevance)
                .ThenByDescending(r => r.CreatedAt)
                .ToList();

            var totalResults = sortedResults.Count;
            var pagedResults = sortedResults
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new SearchResultViewModel
            {
                Query = query,
                Results = pagedResults,
                TotalResults = totalResults,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalResults / pageSize),
                LanguageCode = languageCode.Value   
            };
        }

        public async Task<List<string>> GetSearchSuggestionsAsync(string query, Language? languageCode = null, int maxResults = 5)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return new List<string>();
            }

            if (languageCode == null)
            {
                languageCode = await _languageService.GetCurrentLanguageAsync();
            }

            using var _context = await _contextFactory.CreateDbContextAsync();  
			var suggestions = new List<string>();

            // Seiten-Titel vorschlagen
            var seitenTitel = await _context.Seiten
                .Where(st => st.Language == languageCode &&
                           st.IstVeroeffentlicht &&
                           st.Titel.ToLower().Contains(query.ToLower()))
                .Select(st => st.Titel)
                .Take(maxResults)
                .ToListAsync();
            suggestions.AddRange(seitenTitel);

            // Blog-Post-Titel vorschlagen
            if (suggestions.Count < maxResults)
            {
                var blogTitel = await _context.Posts
                    .Where(bt => bt.Language == languageCode &&
                               bt.IstVeroeffentlicht &&
                               bt.Titel.ToLower().Contains(query.ToLower()))
                    .Select(bt => bt.Titel)
                    .Take(maxResults - suggestions.Count)
                    .ToListAsync();
                suggestions.AddRange(blogTitel);
            }

            return suggestions.Distinct().Take(maxResults).ToList();
        }

        public async Task IndexContentAsync()
        {
            // Für zukünftige Implementierung eines Suchindex
            // Hier könnte man Elasticsearch oder Lucene.NET integrieren
            await Task.CompletedTask;
        }

        private async Task<List<SearchResultItemViewModel>> SearchSeitenAsync(List<string> searchTerms, Language languageId)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();      
			var query = _context.Seiten
                .Where(st => st.Language == languageId && st.IstVeroeffentlicht);

            var results = new List<SearchResultItemViewModel>();

            foreach (var translation in await query.ToListAsync())
            {
                var relevance = CalculateRelevance(searchTerms, translation.Titel, translation.Inhalt, translation.MetaDescription);
                if (relevance > 0)
                {
                    results.Add(new SearchResultItemViewModel
                    {
                        Type = "Seite",
                        Title = translation.Titel,
                        Excerpt = CreateExcerpt(translation.Inhalt, searchTerms.First()),
                        Url = $"/seite/{translation.Slug}",
                        Relevance = relevance,
                        CreatedAt = translation.ErstelltAm
                    });
                }
            }

            return results;
        }

        private async Task<List<SearchResultItemViewModel>> SearchPostsAsync(List<string> searchTerms, Language languageId)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();      
			var query = _context.Posts
                
                .Where(bt => bt.Language == languageId && bt.IstVeroeffentlicht);

            var results = new List<SearchResultItemViewModel>();

            foreach (var translation in await query.ToListAsync())
            {
                var relevance = CalculateRelevance(searchTerms, translation.Titel, translation.Inhalt, translation.MetaDescription);
                if (relevance > 0)
                {
                    results.Add(new SearchResultItemViewModel
                    {
                        Type = "Blog-Post",
                        Title = translation.Titel,
                        Excerpt = string.IsNullOrEmpty(translation.Excerpt)
                            ? CreateExcerpt(translation.Inhalt, searchTerms.First())
                            : translation.Excerpt,
                        Url = $"/blog/{translation.Slug}",
                        Relevance = relevance,
                        CreatedAt = translation.ErstelltAm
                    });
                }
            }

            return results;
        }

        private async Task<List<SearchResultItemViewModel>> SearchKategorienAsync(List<string> searchTerms, Language languageId)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();  
			var query = _context.Kategorien
                
                .Where(kt => kt.Language == languageId);

            var results = new List<SearchResultItemViewModel>();

            foreach (var translation in await query.ToListAsync())
            {
                var relevance = CalculateRelevance(searchTerms, translation.Name, translation.Beschreibung, null);
                if (relevance > 0)
                {
                    results.Add(new SearchResultItemViewModel
                    {
                        Type = "Kategorie",
                        Title = translation.Name,
                        Excerpt = translation.Beschreibung ?? "",
                        Url = $"/blog/kategorie/{translation.Slug}",
                        Relevance = relevance,
                        CreatedAt = translation.ErstelltAm
                    });
                }
            }

            return results;
        }

        private List<string> PrepareSearchTerms(string query)
        {
            // Suchbegriffe normalisieren und aufteilen
            var cleanQuery = Regex.Replace(query.ToLower(), @"[^\w\s]", " ");
            var terms = cleanQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(term => term.Length > 2)
                .Distinct()
                .ToList();

            return terms;
        }

        private int CalculateRelevance(List<string> searchTerms, string title, string? content, string? metaDescription)
        {
            int relevance = 0;
            var titleLower = title.ToLower();
            var contentLower = content?.ToLower() ?? "";
            var metaLower = metaDescription?.ToLower() ?? "";

            foreach (var term in searchTerms)
            {
                // Titel hat höchste Relevanz
                if (titleLower.Contains(term))
                {
                    relevance += titleLower.Equals(term) ? 100 : 50; // Exakter Match vs. Teil-Match
                }

                // Meta-Description hat mittlere Relevanz
                if (metaLower.Contains(term))
                {
                    relevance += 25;
                }

                // Content hat niedrigste Relevanz
                if (contentLower.Contains(term))
                {
                    relevance += 10;
                }
            }

            return relevance;
        }

        private string CreateExcerpt(string? content, string searchTerm, int maxLength = 200)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            // HTML-Tags entfernen
            var plainText = Regex.Replace(content, @"<[^>]+>", "");

            if (plainText.Length <= maxLength)
                return plainText;

            // Suchen nach dem ersten Vorkommen des Suchbegriffs
            var termIndex = plainText.ToLower().IndexOf(searchTerm.ToLower());

            if (termIndex == -1)
            {
                // Wenn Suchbegriff nicht gefunden, ersten Teil zurückgeben
                return plainText.Substring(0, Math.Min(maxLength, plainText.Length)) + "...";
            }

            // Excerpt um den Suchbegriff herum erstellen
            var startIndex = Math.Max(0, termIndex - maxLength / 2);
            var length = Math.Min(maxLength, plainText.Length - startIndex);

            var excerpt = plainText.Substring(startIndex, length);

            if (startIndex > 0)
                excerpt = "..." + excerpt;

            if (startIndex + length < plainText.Length)
                excerpt = excerpt + "...";

            return excerpt;
        }
    }
}
