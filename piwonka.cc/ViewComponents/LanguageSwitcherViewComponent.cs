
using Microsoft.AspNetCore.Mvc;
using Piwonka.CC.Services;
using Piwonka.CC.Models;

namespace Piwonka.CC.ViewComponents
{
    public class LanguageSwitcherViewComponent : ViewComponent
    {
        private readonly ILanguageService _languageService;

        public LanguageSwitcherViewComponent(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string cssClass = "")
        {
            var viewModel = new LanguageSwitcherViewModel
            {
                CurrentLanguage = await _languageService.GetCurrentLanguageAsync(),
                AvailableLanguages = await _languageService.GetActiveLanguagesAsync(),
                CssClass = cssClass
            };

            return View(viewModel);
        }
    }

    // ViewModel für den ViewComponent
    public class LanguageSwitcherViewModel
    {
        public Language CurrentLanguage { get; set; }
        public List<Language> AvailableLanguages { get; set; } = new();
        public string CssClass { get; set; } = "";
    }
}