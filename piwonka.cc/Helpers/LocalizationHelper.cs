// Helpers/LocalizationHelper.cs
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Helpers
{
    public static class LocalizationHelper
    {
        /// <summary>
        /// Extension-Methode für IHtmlHelper zur einfacheren Lokalisierung
        /// </summary>
        public static async Task<IHtmlContent> LocalizeAsync(this IHtmlHelper htmlHelper, string key)
        {
            var localizationService = htmlHelper.ViewContext.HttpContext.RequestServices
                .GetService<ILocalizationService>();

            if (localizationService != null)
            {
                var localizedString = await localizationService.GetLocalizedStringAsync(key);
                return new HtmlString(localizedString);
            }

            return new HtmlString(key);
        }

        /// <summary>
        /// Extension-Methode für IHtmlHelper zur Lokalisierung mit spezifischer Sprache
        /// </summary>
        public static async Task<IHtmlContent> LocalizeAsync(this IHtmlHelper htmlHelper, string key, Language language)
        {
            var localizationService = htmlHelper.ViewContext.HttpContext.RequestServices
                .GetService<ILocalizationService>();

            if (localizationService != null)
            {
                var localizedString = await localizationService.GetLocalizedStringAsync(key, language);
                return new HtmlString(localizedString);
            }

            return new HtmlString(key);
        }

        /// <summary>
        /// Formatiert Datumsangaben sprachspezifisch
        /// </summary>
        public static string FormatDate(this DateTime date, Language language)
        {
            return language switch
            {
                Language.DE => date.ToString("dd. MMMM yyyy", new System.Globalization.CultureInfo("de-DE")),
                Language.EN => date.ToString("MMMM dd, yyyy", new System.Globalization.CultureInfo("en-US")),
                _ => date.ToString("dd. MMMM yyyy", new System.Globalization.CultureInfo("de-DE"))
            };
        }

        /// <summary>
        /// Formatiert kurze Datumsangaben sprachspezifisch
        /// </summary>
        public static string FormatShortDate(this DateTime date, Language language)
        {
            return language switch
            {
                Language.DE => date.ToString("dd.MM.yyyy", new System.Globalization.CultureInfo("de-DE")),
                Language.EN => date.ToString("MM/dd/yyyy", new System.Globalization.CultureInfo("en-US")),
                _ => date.ToString("dd.MM.yyyy", new System.Globalization.CultureInfo("de-DE"))
            };
        }

        /// <summary>
        /// Formatiert mittlere Datumsangaben sprachspezifisch
        /// </summary>
        public static string FormatMediumDate(this DateTime date, Language language)
        {
            return language switch
            {
                Language.DE => date.ToString("dd. MMM yyyy", new System.Globalization.CultureInfo("de-DE")),
                Language.EN => date.ToString("MMM dd, yyyy", new System.Globalization.CultureInfo("en-US")),
                _ => date.ToString("dd. MMM yyyy", new System.Globalization.CultureInfo("de-DE"))
            };
        }

        /// <summary>
        /// Gibt die richtige Pluralform zurück
        /// </summary>
        public static string GetPlural(this int count, Language language, string singularKey, string pluralKey)
        {
            return count == 1 ? singularKey : pluralKey;
        }

        /// <summary>
        /// Erstellt lokalisierte URL-Parameter
        /// </summary>
        public static string CreateLocalizedUrl(this IHtmlHelper htmlHelper, string page, object routeValues = null)
        {
            var languageService = htmlHelper.ViewContext.HttpContext.RequestServices
                .GetService<ILanguageService>();

            // Hier könnten Sie sprachspezifische URL-Logik implementieren
            // z.B. /de/blog oder /en/blog

            return htmlHelper.ViewContext.HttpContext.Request.PathBase + page;
        }
    }
}