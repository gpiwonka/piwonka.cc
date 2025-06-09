// Services/LanguageService.cs - Neue Klasse
using System.Collections.Generic;

namespace Piwonka.CC.Services
{
    public class LanguageService
    {
        public static readonly Dictionary<string, string> SupportedLanguages = new()
        {
            { "de", "Deutsch" },
            { "en", "English" },
            { "fr", "Français" },
            { "it", "Italiano" }
        };

        public static string GetDefaultLanguage()
        {
            return "de";
        }

        public static string GetCurrentLanguage(HttpContext httpContext)
        {
            // Sprache aus URL-Parameter
            var langFromQuery = httpContext.Request.Query["lang"].FirstOrDefault();
            if (!string.IsNullOrEmpty(langFromQuery) && SupportedLanguages.ContainsKey(langFromQuery))
            {
                httpContext.Session.SetString("SelectedLanguage", langFromQuery);
                return langFromQuery;
            }

            // Sprache aus Session
            var langFromSession = httpContext.Session.GetString("SelectedLanguage");
            if (!string.IsNullOrEmpty(langFromSession) && SupportedLanguages.ContainsKey(langFromSession))
            {
                return langFromSession;
            }

            return GetDefaultLanguage();
        }

        public static void SetCurrentLanguage(HttpContext httpContext, string language)
        {
            if (SupportedLanguages.ContainsKey(language))
            {
                httpContext.Session.SetString("SelectedLanguage", language);
            }
        }
    }
}