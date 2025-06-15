// ViewComponents/SimpleCookieBannerViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using Piwonka.CC.Services;

namespace Piwonka.CC.ViewComponents
{
    public class SimpleCookieBannerViewComponent : ViewComponent
    {
        private readonly ISimpleCookieService _cookieService;
        private readonly ILanguageService _languageService;

        public SimpleCookieBannerViewComponent(
            ISimpleCookieService cookieService,
            ILanguageService languageService)
        {
            _cookieService = cookieService;
            _languageService = languageService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_cookieService.ShouldShowBanner(HttpContext))
            {
                return Content(string.Empty);
            }

            var currentLanguage = await _languageService.GetCurrentLanguageAsync();

            var texts = GetTexts(currentLanguage);
            ViewBag.Texts = texts;

            return View();
        }

        private Dictionary<string, string> GetTexts(Models.Language language)
        {
            if (language == Models.Language.EN)
            {
                return new Dictionary<string, string>
                {
                    ["Message"] = "This website uses essential cookies to ensure proper functionality. By continuing to use this site, you accept our use of cookies.",
                    ["Accept"] = "Accept",
                    ["PrivacyPolicy"] = "Privacy Policy"
                };
            }

            return new Dictionary<string, string>
            {
                ["Message"] = "Diese Website verwendet notwendige Cookies, um eine ordnungsgemäße Funktion zu gewährleisten. Durch die weitere Nutzung dieser Website akzeptieren Sie unsere Verwendung von Cookies.",
                ["Accept"] = "Akzeptieren",
                ["PrivacyPolicy"] = "Datenschutzerklärung"
            };
        }
    }
}