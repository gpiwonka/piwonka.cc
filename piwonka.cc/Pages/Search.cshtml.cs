using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piwonka.CC.Models;
using Piwonka.CC.Services;
using Piwonka.CC.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages
{
    public class SearchModel : PageModel
    {
        private readonly ISearchService _searchService;
        private readonly ILanguageService _languageService;

        public SearchModel(ISearchService searchService, ILanguageService languageService)
        {
            _searchService = searchService;
            _languageService = languageService;
        }

        [BindProperty(SupportsGet = true)]
        public SearchFormViewModel SearchForm { get; set; } = new();

        public SearchResultViewModel SearchResult { get; set; } = new();
        public List<Language> AvailableLanguages { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? query, string? lang, int page = 1)
        {
            AvailableLanguages = await _languageService.GetActiveLanguagesAsync();

            if (!string.IsNullOrEmpty(lang))
            {
                await _languageService.SetCurrentLanguageAsync(lang);
            }

            if (!string.IsNullOrEmpty(query))
            {
                SearchForm.Query = query;
                SearchForm.LanguageCode = await _languageService.GetCurrentLanguageAsync();

                SearchResult = await _searchService.SearchAsync(query, SearchForm.LanguageCode, page);
            }

            return Page();
        }
    }
}
